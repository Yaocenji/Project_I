using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project_I.AVGpart
{
    // 场景中的人物信息与立绘管理器：就是当前显示的内容
    [Serializable]
    public class sceneCharacter
    {
        public CharacterInfo characterInfo;
        public string characterName;
        public Image currIllustration;
        public Image currHeadIllustration;
    }
    
    // 场景中的声音管理器
    [Serializable]
    public class sceneSound
    {
        //public SoundInfo soundInfo;
        public AudioSource audioSource;
        public string soundName;
        public bool loop;
        public bool playing;
    }
    
    public class PlotScenePlayer : MonoBehaviour
    {
        static public PlotScenePlayer Instance;
        
        // 是否正在播放中（当淡入淡出等动画播放到一半是，该值为true）
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying;
        }
        
        // 演出用的序列列表
        private LinkedList<PerformMoveSequence> moveSequenceList;
        
        [Header("立绘与头像")]
        // 角色立绘序列的父
        public GameObject illustrationParentGameObject;
        // 角色头像立绘序列的父
        public GameObject headIllustrationParentGameObject;
        
        [Header("声音与音乐播放器")]
        public GameObject soundPlayerParentGameObject;
        
        // Avg演出所用的各项内容：
        [Header("摄像机")]
        public Camera cam;
        [Header("章节标题字幕text")]
        public TextMeshProUGUI ChapterCaption;
        [Header("章节标题canvas group")]
        public CanvasGroup CaptionCanvasGroup;
        [Header("CG图")]
        public Image CgImg;
        [Header("背景图")]
        public Image BackgroundImg;
        [Header("对话框人名")]
        public TextMeshProUGUI DialogName;
        [Header("对话框text")]
        public TextMeshProUGUI DialogText;
        [Header("对话框canvas group")]
        public CanvasGroup DialogCanvasGroup;

        [Header("锚点")]
        public Transform HeadIllustrationTop;
        public Transform IdleIllustrationPos;
        public Transform IllustrationLeft;
        public Transform IllustrationMiddle;
        public Transform IllustrationRight;
        
        // 场景中出现的角色
        private List<sceneCharacter> characters;
        // 场景中出现的声音
        private List<sceneSound> sounds;

        public RectTransform test;
        
        private void Awake()
        {
            Instance = this;
            
            characters =  new List<sceneCharacter>();
            sounds = new List<sceneSound>();
            moveSequenceList = new LinkedList<PerformMoveSequence>();
        }

        private void Start()
        {
        }

        public void Update()
        {
            // 更新当前的播放状态
            if (!isPlaying && moveSequenceList.Count >= 0)   // 这一帧之前没有运行，这一帧时开始运行
            {
                isPlaying = true;
                PlayBegin();
            }
            

            // 如果当前队列列表有任何演出队列
            if (moveSequenceList.Count > 0)
            {
                // 准备一个记录应被去除的表
                HashSet<PerformMoveSequence> removeList = new HashSet<PerformMoveSequence>();
                // 获取帧时间
                float deltaTime = Time.deltaTime;
                // 遍历存在的演出队列
                foreach (PerformMoveSequence seq in moveSequenceList)
                {
                    // 若该队列尚未结束
                    if (!seq.isCompleted)
                    {
                        // 立即调用该队列
                        float dt = deltaTime;
                        seq.Execute(ref dt);
                    }
                    // 若该队列已经结束
                    else
                    {
                        // 标记为需要移除。
                        removeList.Add(seq);
                    }
                }
                // 移除被标记移除的
                foreach (PerformMoveSequence seq in removeList)
                {
                    moveSequenceList.Remove(seq);
                }
                removeList.Clear();
            }

            // 更新当前的播放状态
            if (isPlaying && moveSequenceList.Count == 0)   // 这一帧之前正在运行，这一帧时结束运行
            {
                isPlaying = false;
                PlayEnd();
            }
        }

        public void PlayBegin()
        {
            // TODO
        }
        public void PlayEnd()
        {
            //Debug.Log("这一句播放结束。");
            PlotSceneManager.Instance.PlayEnd();
        }
        
        void DeleteChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
        
        // 初始化
        public void InitPlay()
        {
            // 清空所有的立绘和头像
            DeleteChildren(illustrationParentGameObject.transform);
            DeleteChildren(headIllustrationParentGameObject.transform);
            
            // 设置所有的文字为空
            ChapterCaption.text = "";
            DialogText.text = "";
            DialogName.text = "";
            
            // 设置所有的透明度为0
            CaptionCanvasGroup.alpha = 0;
            DialogCanvasGroup.alpha = 0;
            
            CgImg.color = new Color(CgImg.color.r, CgImg.color.g, CgImg.color.b, 0);
            CgImg.sprite = null;
            
            BackgroundImg.color = new Color(BackgroundImg.color.r, BackgroundImg.color.g, BackgroundImg.color.b, 0);
            BackgroundImg.sprite = null;
            
            // 清空场景角色
            characters.Clear();
            
            moveSequenceList.Clear();
        }

        // 开一个章节标题
        public void DisplayChapter(string chapterName)
        {
            ChapterCaption.text = chapterName;
            // 向linkedList里面添加黑幕淡入淡出的演出
            
            // 轨道一：转场幕布的淡入淡出
            PerformMove_CanvasGroupFadeTo blackFadin = new PerformMove_CanvasGroupFadeTo(CaptionCanvasGroup, 1, 1.5F);
            PerformMove_DoNothing blackDnt = new PerformMove_DoNothing(3f);
            PerformMove_CanvasGroupFadeTo blackFadout = new PerformMove_CanvasGroupFadeTo(CaptionCanvasGroup, 0f, 1.5F);
            
            PerformMoveSequence blackSequence = new PerformMoveSequence();
            blackSequence.moves.Add(blackFadin);
            blackSequence.moves.Add(blackDnt);
            blackSequence.moves.Add(blackFadout);

            // 将这个轨道添加到轨道列表
            moveSequenceList.AddLast(new LinkedListNode<PerformMoveSequence>(blackSequence));
        }

        public void EndChapter()
        {
            ChapterCaption.text = "";
            
            // 轨道一：转场幕布的淡入淡出
            PerformMove_CanvasGroupFadeTo blackFadin = new PerformMove_CanvasGroupFadeTo(CaptionCanvasGroup, 1, 1.5F);
            blackFadin.Callback = () => InitPlay();
            
            PerformMoveSequence blackSequence = new PerformMoveSequence();
            blackSequence.moves.Add(blackFadin);
        }

        // 添加这一场的角色
        public void AddCharacter(string characterName)
        {
            CharacterInfo targetCharacter = null;
            foreach (var characterInfo in CharacterManager.Instance.data.charactorInfos)
            {
                if (characterName.Equals(characterInfo.Name))
                {
                    targetCharacter = characterInfo;
                    break;
                }
            }

            if (targetCharacter is not null)
            {
                sceneCharacter newCharacter = new sceneCharacter();
                newCharacter.characterInfo = targetCharacter;
                newCharacter.characterName = characterName;
                
                characters.Add(newCharacter);
            }
        }

        // 为角色设置立绘
        public void SetIllustration
        (string characterName, string clothingName, string postureName, string expressionName)
        {
            sceneCharacter targetCharacter = null;
            foreach (var sC in characters)
            {
                if (sC.characterInfo.Name.Equals(characterName))
                {
                    targetCharacter = sC;
                    break;
                }
            }

            if (targetCharacter is null)
            {
                // TODO 报错应该发送给PlotSceneManager
                //Debug.LogError("错误：在添加角色前试图设置角色立绘！ 错误行：角本" + );
            }

            if (targetCharacter is not null)
            {
                var foundSprite =
                    CharacterManager.Instance.data.FindIllustrationSprite(characterName, clothingName, postureName,
                        expressionName);
                if (foundSprite is not null)
                {
                    
                    // 创建新gameobject 和 image
                    GameObject newCharacterIllustrationGO = new GameObject();
                    newCharacterIllustrationGO.name = characterName + "_立绘";
                    newCharacterIllustrationGO.transform.SetParent(illustrationParentGameObject.transform);
                    // 初始化位置
                    newCharacterIllustrationGO.transform.position = new Vector3(-1000, 0, 0);
                
                    newCharacterIllustrationGO.AddComponent<Image>();
                
                    GameObject newCharacterHeadIllustrationGO = new GameObject();
                    newCharacterHeadIllustrationGO.name = characterName + "_头像";
                    newCharacterHeadIllustrationGO.transform.SetParent(headIllustrationParentGameObject.transform);
                    // 初始化位置
                    newCharacterHeadIllustrationGO.transform.position = new Vector3(-1000, 0, 0);
                    newCharacterHeadIllustrationGO.AddComponent<Image>();
                    
                    targetCharacter.currIllustration = newCharacterIllustrationGO.GetComponent<Image>();
                    targetCharacter.currHeadIllustration =  newCharacterHeadIllustrationGO.GetComponent<Image>();
                    
                    
                    targetCharacter.currIllustration.sprite = foundSprite;
                    targetCharacter.currHeadIllustration.sprite = foundSprite;
                    // 还要设置正确的大小
                    targetCharacter.currIllustration.GetComponent<RectTransform>().sizeDelta = foundSprite.rect.size;
                    targetCharacter.currHeadIllustration.GetComponent<RectTransform>().sizeDelta = foundSprite.rect.size;
                    // 关键点：头像的锚点设置到顶部
                    targetCharacter.currHeadIllustration.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.0f);
                }
            }
        }

        // 设置背景图
        public void SetBackground(string backgroundName, string diffName)
        {
            var fundSprite = BackgroundImageManager.Instance.data.FindIllustrationSprite(backgroundName, diffName);
            if (fundSprite is not null)
            {
                // 这个要添加一条轨道
                PerformMoveSequence setBackgroundSequence = new PerformMoveSequence();

                // 当前背景淡出
                PerformMove_ImageFadeTo bgdFadeOut = new PerformMove_ImageFadeTo(BackgroundImg, 0, 1.5f);
                // 切换背景图
                PerformMove_SwitchIllustration switchIllustration =
                    new PerformMove_SwitchIllustration(BackgroundImg, fundSprite);
                // 当前背景淡入
                PerformMove_ImageFadeTo bgdFadeIn = new PerformMove_ImageFadeTo(BackgroundImg, 1, 1.5F);

                setBackgroundSequence.moves.Add(bgdFadeOut);
                setBackgroundSequence.moves.Add(switchIllustration);
                setBackgroundSequence.moves.Add(bgdFadeIn);

                moveSequenceList.AddLast(setBackgroundSequence);
            }
        }

        // 设置CG图
        public void SetCG(string cgName, string diffName)
        {
            var fundSprite = CgManager.Instance.data.FindIllustrationSprite(backgroundName, diffName);
            if (fundSprite is not null)
            {
                // 这个要添加一条轨道
                PerformMoveSequence setBackgroundSequence = new PerformMoveSequence();

                // 当前背景淡出
                PerformMove_ImageFadeTo bgdFadeOut = new PerformMove_ImageFadeTo(CgImg, 0, 1.5f);
                // 切换背景图
                PerformMove_SwitchIllustration switchIllustration =
                    new PerformMove_SwitchIllustration(CgImg, fundSprite);
                // 当前背景淡入
                PerformMove_ImageFadeTo bgdFadeIn = new PerformMove_ImageFadeTo(CgImg, 1, 1.5F);

                setBackgroundSequence.moves.Add(bgdFadeOut);
                setBackgroundSequence.moves.Add(switchIllustration);
                setBackgroundSequence.moves.Add(bgdFadeIn);

                moveSequenceList.AddLast(setBackgroundSequence);
            }
        }
        // 关闭CG图
        public void DisableCG(string cgName, string diffName)
        {
            if (fundSprite is not null)
            {
                // 这个要添加一条轨道
                PerformMoveSequence setBackgroundSequence = new PerformMoveSequence();

                // 当前背景淡出
                PerformMove_ImageFadeTo bgdFadeOut = new PerformMove_ImageFadeTo(CgImg, 0, 1.5f);
                // 切换背景图
                Action switchCGToNull = () => { CgImg.sprite = null; };
                bgdFadeOut.Callback = switchCGToNull;
                
                setBackgroundSequence.moves.Add(bgdFadeOut);
                setBackgroundSequence.moves.Add(switchIllustration);
                
                moveSequenceList.AddLast(setBackgroundSequence);
            }
        }
        
        // 开关对话框显示
        public void SetDialogBoxDisplay(bool display)
        {
            // 轨道一：黑幕的淡入淡出
            PerformMove_CanvasGroupFadeTo fadin = new PerformMove_CanvasGroupFadeTo(DialogCanvasGroup, 1, 1);
            
            PerformMoveSequence sequence = new PerformMoveSequence();
            sequence.moves.Add(fadin);
            
            moveSequenceList.AddLast(new LinkedListNode<PerformMoveSequence>(sequence));
        }
        
        // 音乐音效的播放
        public void PlaySound(string soundName, bool loop, float soundVolume, float stereo)
        {
            GameObject newSoundGO = new GameObject();
            newSoundGO.name = soundName;
            newSoundGO.transform.SetParent(soundPlayerParentGameObject.transform);
            
            var newSound = newSoundGO.AddComponent<AudioSource>();
            // 设置音频
            newSound.clip = SoundManager.Instance.data.FindSound(soundName);
            // 设为2D
            newSound.spatialBlend = 0;
            // 设置循环
            newSound.loop = loop;
            // 设置初始音量：0
            newSound.volume = 0;
            // 设置立体声
            newSound.panStereo = stereo;
            
            // 启动！
            newSound.Play();
            
            sceneSound newSceneSound = new sceneSound();
            newSceneSound.soundName = soundName;
            newSceneSound.loop = loop;
            newSceneSound.audioSource = newSound;
            newSceneSound.playing = true;
            sounds.Add(newSceneSound);
            
            // 添加一个轨道：声音淡入
            PerformMove_AudioVolumeFadeTo audioVolumeFadeTo = new PerformMove_AudioVolumeFadeTo(newSound, soundVolume, 1);
            
            PerformMoveSequence sequence = new PerformMoveSequence();
            sequence.moves.Add(audioVolumeFadeTo);
            moveSequenceList.AddLast(sequence);
        }
        
        // 音乐音效的停止
        public void EndSound(string soundName)
        {
            sceneSound targetSS = null;
            foreach (var ss in sounds)
            {
                if (ss.soundName == soundName)
                {
                    targetSS = ss;
                    
                    // 添加一个轨道：声音淡出
                    PerformMove_AudioVolumeFadeTo audioVolumeFadeTo = new PerformMove_AudioVolumeFadeTo(ss.audioSource, 0, 0.75f);

                    audioVolumeFadeTo.Callback = () => { Destroy(ss.audioSource.gameObject); };
            
                    PerformMoveSequence sequence = new PerformMoveSequence();
                    sequence.moves.Add(audioVolumeFadeTo);
                    moveSequenceList.AddLast(sequence);
                }
                break;
            }

            if (targetSS is not null)
            {
                sounds.Remove(targetSS);
            }
        }
        
        // 心理活动或旁白
        public void DisplayNarrator(string text)
        {
            DialogName.text = "";
            
            foreach (var sc in characters)
            {
                if (sc is null || sc.currHeadIllustration is null || sc.currHeadIllustration is null)
                {
                    // TODO Temporally Do Nothing.
                }
                else
                {
                    // 闲置的位置
                    sc.currHeadIllustration.GetComponent<RectTransform>().position = IdleIllustrationPos.position;
                }
            }
            
            // 轨道：打字机
            PerformMove_TextTypewritter typewritter = new PerformMove_TextTypewritter(text, DialogText, 0.05f);
            
            PerformMoveSequence sequence = new PerformMoveSequence();
            sequence.moves.Add(typewritter);
            
            moveSequenceList.AddLast(sequence);
        }

        // 对话
        public void DisplayDialog(string characterName, string text)
        {
            DialogName.text = characterName;

            // 轨道：打字机
            PerformMove_TextTypewritter typewritter = new PerformMove_TextTypewritter(text, DialogText, 0.05f);
            PerformMoveSequence textSequence = new PerformMoveSequence();
            textSequence.moves.Add(typewritter);
            moveSequenceList.AddLast(textSequence);

            // 人物头像
            sceneCharacter thisCharacter = null;
            foreach (var sc in characters)
            {
                if (sc.characterName.Equals(characterName))
                {
                    thisCharacter = sc;
                    break;
                }
            }

            foreach (var sc in characters)
            {
                if (sc == thisCharacter)
                {
                    if (sc is null || sc.currHeadIllustration is null || sc.currHeadIllustration is null)
                    {
                        // TODO Temporally Do Nothing.
                    }
                    else
                    {
                        // 出现在正确的位置
                        sc.currHeadIllustration.GetComponent<RectTransform>().position = HeadIllustrationTop.position;
                        // 透明度为1
                        sc.currHeadIllustration.color =
                            new Color(sc.currHeadIllustration.color.r,
                                sc.currHeadIllustration.color.g,
                                sc.currHeadIllustration.color.b, 1.0f);
                    }
                }
                else
                {
                    if (sc is null || sc.currHeadIllustration is null || sc.currHeadIllustration is null)
                    {
                        // TODO Temporally Do Nothing.
                    }
                    else
                    {
                        // 闲置的位置
                        sc.currHeadIllustration.GetComponent<RectTransform>().position = IdleIllustrationPos.position;
                    }
                }
            }

        }
    
        // 立绘位置
        public void SetIllustrationPosition(string characterName, int pos)
        {
            sceneCharacter targetSC = null;
            foreach (var sc in characters)
            {
                if (characterName.Equals(sc.characterName))
                {
                    targetSC = sc;
                    break;
                }
            }

            if (targetSC.currIllustration is not null)
            {
                if (pos == 0)
                    targetSC.currIllustration.GetComponent<Transform>().position = IllustrationLeft;

                if (pos == 1)
                    targetSC.currIllustration.GetComponent<Transform>().position = IllustrationMiddle;

                if (pos == 2)
                    targetSC.currIllustration.GetComponent<Transform>().position = IllustrationRight;
            }
        } 
    
    }
}