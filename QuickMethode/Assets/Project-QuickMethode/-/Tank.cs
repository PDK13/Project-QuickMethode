using QuickMethode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    [SerializeField] private float m_deg = 30f;
    [SerializeField] private float m_power = 100f;
    [SerializeField] private Transform m_start;

    private LineRenderer m_lineRenderer;
    private RendererTrajectory m_rendererTrajectory;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_rendererTrajectory = GetComponent<RendererTrajectory>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3[] Line = GetTrajectoryPoints(m_deg, m_power, m_start.transform.position).ToArray();
            m_lineRenderer.positionCount = Line.Length;
            m_lineRenderer.SetPositions(Line);
        }
    }

    public List<Vector3> GetTrajectoryPoints(float Deg, float Power, Vector2 PosStart, float GravityScale = 1, float RigidbodyDrag = 0)
    {
        List<Vector3> Trajectory = new List<Vector3>();

        float TimeStep = Time.fixedDeltaTime / Physics.defaultSolverVelocityIterations;
        Vector3 GravityAccel = -Physics2D.gravity * Vector2.down * GravityScale * TimeStep * TimeStep;
        float TimeDrag = 1f - TimeStep * RigidbodyDrag;
        Vector3 TrajectoryDir = QCircle.GetPosXY(Deg, 1f).normalized * Power;
        Vector3 MoveStep = TrajectoryDir * TimeStep;
        Vector3 PosPoint = PosStart;
        Trajectory.Add(PosPoint);
        for (int i = 0; i < 500; i++)
        {
            MoveStep += GravityAccel;
            MoveStep *= TimeDrag;
            PosPoint += MoveStep;
            Trajectory.Add(PosPoint);
        }

        return Trajectory;
    }
}
