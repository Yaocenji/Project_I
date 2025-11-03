using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project_I.Bot
{
    // 节点状态
    public enum NodeStatus
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    // 节点基类
    public abstract class BehaviorTreeNode
    {
        public BehaviorTree tree;
        
        protected NodeStatus status;

        public string nodeName = "abcNode";
        public abstract NodeStatus Tick();
    }

    // 组合节点：序列器
    [NodeName("组合|序列器")]
    [NodeChildLimit(int.MaxValue)]
    public class SequenceNode : BehaviorTreeNode
    {
        public List<BehaviorTreeNode> children = new List<BehaviorTreeNode>();

        public SequenceNode(List<BehaviorTreeNode> children)
        {
            this.children = children;
        }

        public override NodeStatus Tick()
        {
            foreach (var node in children)
            {
                switch (node.Tick())
                {
                    case NodeStatus.RUNNING:
                        status = NodeStatus.RUNNING;
                        return status;
                    case NodeStatus.SUCCESS:
                        continue;
                    case NodeStatus.FAILURE:
                        status = NodeStatus.FAILURE;
                        return status;
                }
            }

            status = NodeStatus.SUCCESS;
            return status;
        }
    }

    // 组合节点：选择器
    [NodeName("组合|选择器")]
    [NodeChildLimit(int.MaxValue)]
    public class SelectNode : BehaviorTreeNode
    {
        public List<BehaviorTreeNode> children;

        public SelectNode()
        {
            children = new List<BehaviorTreeNode>();
        }
        
        public SelectNode(List<BehaviorTreeNode> children)
        {
            this.children = children;
        }

        public override NodeStatus Tick()
        {
            foreach (var node in children)
            {
                switch (node.Tick())
                {
                    case NodeStatus.RUNNING:
                        status = NodeStatus.RUNNING;
                        return status;
                    case NodeStatus.SUCCESS:
                        status = NodeStatus.SUCCESS;
                        return status;
                    case NodeStatus.FAILURE:
                        continue;
                }
            }

            status = NodeStatus.FAILURE;
            return status;
        }
    }

    // 组合节点：并行器
    [NodeName("组合|并行器")]
    [NodeChildLimit(int.MaxValue)]
    public class ParallelNode : BehaviorTreeNode
    {
        public List<BehaviorTreeNode> children;


        public ParallelNode()
        {
            children = new List<BehaviorTreeNode>();
        }
        public ParallelNode(List<BehaviorTreeNode> children)
        {
            this.children = children;
        }

        public override NodeStatus Tick()
        {
            bool isAnyChildRunning = false;
            
            // 并行器核心逻辑：
            // 必须所有子节点都成功，才算成功
            
            foreach (var node in children)
            {
                var result = node.Tick();
                if (result == NodeStatus.FAILURE)
                {
                    status = NodeStatus.FAILURE;
                    return status;
                }

                if (result == NodeStatus.RUNNING)
                {
                    isAnyChildRunning = true;
                }
            }

            status = isAnyChildRunning ? NodeStatus.RUNNING : NodeStatus.SUCCESS;
            return status;
        }
    }
    
    // 修饰节点：循环器 不断执行子节点，直到执行了预设的次数或者子节点执行失败。
    [NodeName("修饰|循环器")]
    [NodeChildLimit(1)]
    public class LoopNode : BehaviorTreeNode
    {
        public BehaviorTreeNode child;
        
        private bool infinity;
        private int loops;
        private int currentCount;

        public LoopNode(bool infinity, int loops = 1)
        {
            this.child = null;
            this.loops = loops;
            this.infinity = infinity;
            this.currentCount = 0;
        }
        /// <summary>
        /// Executes its child node a specified number of times.
        /// </summary>
        /// <param name="child"></param> The child node to execute.</param>
        /// <param name="loops"></param> The number of times to execute the child.</param>
        public LoopNode(BehaviorTreeNode child, int loops)
        {
            this.child = child;
            this.loops = loops;
            this.infinity = false;
            this.currentCount = 0;
        }
        public LoopNode(BehaviorTreeNode child, bool infinity, int loops = 1)
        {
            this.child = child;
            this.loops = loops;
            this.infinity = infinity;
            this.currentCount = 0;
        }

        public override NodeStatus Tick()
        {
            // Keep executing until we reach the loop count.
            if (currentCount < loops || infinity)
            {
                switch (child.Tick())
                {
                    case NodeStatus.RUNNING:
                        // If the child is running, we are running.
                        status = NodeStatus.RUNNING;
                        break;
                    case NodeStatus.SUCCESS:
                        // The child succeeded, so increment our loop counter.
                        currentCount++;

                        // If we haven't completed all loops, the decorator is still 'running'.
                        if (currentCount < loops)
                        {
                            status = NodeStatus.RUNNING;
                        }
                        // If we have completed all loops, the decorator has succeeded.
                        else
                        {
                            currentCount = 0; // Reset for the next time this node is executed.
                            status = NodeStatus.SUCCESS;
                        }
                        break;
                    case NodeStatus.FAILURE:
                        // If the child fails, the whole loop fails.
                        currentCount = 0; // Reset for the next time.
                        status = NodeStatus.FAILURE;
                        break;
                }
            }
            else // This case is hit if the node is ticked after it has already succeeded.
            {
                currentCount = 0;
                status = NodeStatus.SUCCESS;
            }

            return status;
        }
    }

    // 修饰节点：逆变器
    [NodeName("修饰|逆变器")]
    [NodeChildLimit(1)]
    public class InvertNode : BehaviorTreeNode
    {
        public BehaviorTreeNode child;

        public InvertNode(BehaviorTreeNode child)
        {
            this.child = child;
        }
        public InvertNode()
        {
            this.child = null;
        }

        public override NodeStatus Tick()
        {
            switch (child.Tick())
            {
                case NodeStatus.RUNNING:
                    status = NodeStatus.RUNNING;
                    break;
                case NodeStatus.SUCCESS:
                    status = NodeStatus.FAILURE;
                    break;
                case NodeStatus.FAILURE:
                    status = NodeStatus.SUCCESS;
                    break;
            }

            return status;
        }
    }

    // 条件节点基类
    [NodeName("条件|条件基")]
    [NodeChildLimit(0)]
    public abstract class ConditionNode : BehaviorTreeNode
    {
        // The Tick method is sealed to ensure conditions are evaluated in one frame. 不可重写
        public sealed override NodeStatus Tick()
        {
            status = Check() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
            return status;
        }

        protected abstract bool Check();
    }

    
    // 动作节点基类
    [NodeName("动作|动作基")]
    [NodeChildLimit(0)]
    public abstract class ActionNode : BehaviorTreeNode
    {
        // 用于辅助第一次start
        private bool isFirstFrame = true;
        
        // 该次动作结束后，是否需要临时性地修改一次Tick时间间隔？
        public bool UseTemporalTickInterval = false;
        // 若需要临时性地修改一次Tick时间间隔，那么需要知道改成多少？
        public float TemporalTickInterval = 0.3f;
        
        // The Tick method is sealed to ensure conditions are evaluated in one frame. 不可重写
        public sealed override NodeStatus Tick()
        {
            if (isFirstFrame)
            {
                // 动作的起始
                Start();
                isFirstFrame = false;
            }
            status = Check() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
            return status;
        }

        /// <summary>
        /// 动作开始之时
        /// </summary>
        protected abstract void Start();

        /// <summary>
        /// 不断地确认：动作结束了没？
        /// </summary>
        /// <returns></returns>
        protected abstract bool Check();
        
        /// <summary>
        /// 结束之时
        /// </summary>
        protected virtual void Stop()
        {
            // Default stop func: do nothing.
        }

        protected void SetTemporalTickInterval()
        {
            if (tree is not null && UseTemporalTickInterval)
            {
                tree.SetTemporalInterval(TemporalTickInterval);
            }
        }
    }
}
    
    
