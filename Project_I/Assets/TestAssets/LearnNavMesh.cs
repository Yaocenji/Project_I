using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LearnNavMesh : MonoBehaviour
{
    public Transform target;
    //private NavMeshAgent agent;
    
    private List<Vector3> currentPath = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        //agent = GetComponent<NavMeshAgent>();
        if (target is not null)
        {
            /*// 验证目标点是否在导航网格上
            if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogError("目标点不在导航网格上！");
            }*/

            NavMeshHit hit;
            NavMesh.SamplePosition(target.position, out hit, 1.0f, NavMesh.AllAreas);
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
            {
                Vector3[] corners = path.corners;
                currentPath = new List<Vector3>(corners);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (currentPath == null) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < currentPath.Count - 1; i++)
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}
