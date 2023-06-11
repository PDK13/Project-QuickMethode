using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RigidbodyGravity))]
public class RendererTrajectory : MonoBehaviour
{
    //Main Dir is Vector.R

    [Header("Trajectory main")]

    [SerializeField] private float m_TrajectoryPower = 5f;

    [SerializeField] private int m_TrajectoryStep = 500;

    [Header("Trajectory Point")]

    [SerializeField] private Transform m_TransformStart;

    [SerializeField] private Transform m_TransformNext;

    [Header("Trajectory Raycast")]

    [SerializeField] private bool m_TrajectoryRaycast = false;

    [SerializeField] private LayerMask m_TrajectoryRaycastLayerMask;

    [SerializeField] private float m_TrajectoryRaycastSize = 0.5f;

    private RigidbodyGravity m_RigidbodyGravity;

    private void Awake()
    {
        if (GetComponent<RigidbodyGravity>() == null)
        {
            gameObject.AddComponent<RigidbodyGravity>();
        }

        m_RigidbodyGravity = GetComponent<RigidbodyGravity>();
    }

    #region Trajectory Value

    #region Trajectory Start Point and Next Point

    public void SetTrajectoryStart(Transform m_TrajectoryStart)
    {
        this.m_TransformStart = m_TrajectoryStart;
    }

    public void SetTrajectoryStart(Vector3 m_TrajectoryStart)
    {
        m_TransformStart.position = m_TrajectoryStart;
    }

    public void SetTrajectoryStartChance(Vector3 m_TrajectoryStartChance)
    {
        SetTrajectoryStart(GetTrajectoryStart() + m_TrajectoryStartChance);
    }

    public void SetTrajectoryNext(Transform m_TrajectoryNext)
    {
        this.m_TransformNext = m_TrajectoryNext;
    }

    public void SetTrajectoryNext(Vector3 m_TrajectoryNext)
    {
        m_TransformNext.position = m_TrajectoryNext;
    }

    public void SetTrajectoryNextChance(Vector3 m_TrajectoryNextChance)
    {
        SetTrajectoryNext(GetTrajectoryNext() + m_TrajectoryNextChance);
    }

    public Transform GetTrajectoryStartTransform()
    {
        return m_TransformStart;
    }

    public Vector3 GetTrajectoryStart()
    {
        return m_TransformStart.position;
    }

    public Transform GetTrajectoryNextTransform()
    {
        return m_TransformNext;
    }

    public Vector3 GetTrajectoryNext()
    {
        return m_TransformNext.position;
    }

    public float GetTrajectoryLength()
    {
        return GetTrajectoryDirPrimary(false).magnitude;
    }

    #endregion

    #region Trajectory Power

    public void SetTrajectoryPower(float m_TrajectoryPower)
    {
        this.m_TrajectoryPower = m_TrajectoryPower;
    }

    public void SetTrajectoryPowerChance(float m_TrajectoryPowerChance)
    {
        m_TrajectoryPower += m_TrajectoryPowerChance;
    }

    public float GetTrajectoryPower()
    {
        return m_TrajectoryPower;
    }

    #endregion

    #region Trajectory Step

    public void SetTrajectoryStep(int m_TrajectoryStep)
    {
        this.m_TrajectoryStep = m_TrajectoryStep;
    }

    public int GetTrajectoryStep()
    {
        return m_TrajectoryStep;
    }

    #endregion

    #endregion

    #region Trajectory

    public Vector3[] GetTrajectoryPoints(float m_RigidbodyDrag, bool m_Dir_Normalized)
    {
        Vector3[] m_TrajectoryResult;

        List<Vector3> m_TrajectoryResumList = new List<Vector3>();

        float m_TimeStep = Time.fixedDeltaTime / Physics.defaultSolverVelocityIterations;

        Vector3 m_GravityAccel = m_RigidbodyGravity.GetGravityGlobalVector() * m_RigidbodyGravity.GetGravityScale() * m_TimeStep * m_TimeStep;

        float m_Drag = 1f - m_TimeStep * m_RigidbodyDrag;

        Vector3 m_TrajectoryDir = GetTrajectoryDirPrimary(m_Dir_Normalized);

        Vector3 m_MoveStep = m_TrajectoryDir * m_TimeStep;

        Vector3 m_PosPoint = GetTrajectoryStart();

        for (int i = 0; i < m_TrajectoryStep; i++)
        {
            m_MoveStep += m_GravityAccel;

            m_MoveStep *= m_Drag;

            m_PosPoint += m_MoveStep;

            if (m_TrajectoryRaycast)
            {
                bool rayRaycast = Physics.Linecast(m_PosPoint + Vector3.down * m_TrajectoryRaycastSize, m_PosPoint - Vector3.down * m_TrajectoryRaycastSize, m_TrajectoryRaycastLayerMask);

                if (rayRaycast)
                {
                    m_TrajectoryResult = new Vector3[m_TrajectoryResumList.Count];
                    m_TrajectoryResult = m_TrajectoryResumList.ToArray();
                    return m_TrajectoryResult;
                }
                else
                {
                    m_TrajectoryResumList.Add(m_PosPoint);
                }
            }
            else
            {
                m_TrajectoryResumList.Add(m_PosPoint);
            }
        }

        m_TrajectoryResult = new Vector3[m_TrajectoryResumList.Count];
        m_TrajectoryResult = m_TrajectoryResumList.ToArray();
        return m_TrajectoryResult;
    }

    public Vector3 GetTrajectoryDirPrimary(bool m_Dir_Normalized)
    {
        if (m_Dir_Normalized)
        {
            return (GetTrajectoryNext() - GetTrajectoryStart()).normalized * m_TrajectoryPower;
        }

        return (GetTrajectoryNext() - GetTrajectoryStart()) * m_TrajectoryPower;
    }

    public Vector3 GetTrajectoryDir()
    {
        return (GetTrajectoryNext() - GetTrajectoryStart()) * m_TrajectoryPower;
    }

    #endregion

    #region Angle for hit Trajectory

    public float? GetTrajectoryAngleDeg(Vector3 m_PosStart, Vector3 m_PosTarket, bool m_AngleHighAllow)
    {
        //Get the Deg to hit Target!

        Vector3 m_TarketDir = m_PosTarket - m_PosStart;

        float m_Y_High = m_TarketDir.y;

        m_TarketDir.y = 0f;

        float m_XLength = m_TarketDir.magnitude;

        float m_Gravity = m_RigidbodyGravity.GetGravityGlobalFloat() * m_RigidbodyGravity.GetGravityScale();

        float m_Speed_SQR = m_TrajectoryPower * m_TrajectoryPower;

        float m_Under_SQR = (m_Speed_SQR * m_Speed_SQR) - m_Gravity * (m_Gravity * m_XLength * m_XLength + 2 * m_Y_High * m_Speed_SQR);

        if (m_Under_SQR >= 0)
        {
            float m_Under_SQRT = Mathf.Sqrt(m_Under_SQR);
            float m_Angle_High = m_Speed_SQR + m_Under_SQRT;
            float m_AngleLow = m_Speed_SQR - m_Under_SQRT;

            if (m_AngleHighAllow)
            {
                return Mathf.Atan2(m_Angle_High, m_Gravity * m_XLength) * Mathf.Rad2Deg;
            }
            else
            {
                return Mathf.Atan2(m_AngleLow, m_Gravity * m_XLength) * Mathf.Rad2Deg;
            }
        }

        return null;
    }

    #endregion

    #region Rigidbody Velocity with Trajectory
    
    public void SetBullet(Rigidbody m_Rigidbody, Vector3 m_TrajectoryStart, Vector3 m_TrajectoryNext)
    {
        //Set velocity to Bullet!

        if (m_Rigidbody.GetComponent<RigidbodyGravity>() == null)
        {
            m_Rigidbody.gameObject.AddComponent<RigidbodyGravity>();
        }

        m_Rigidbody.GetComponent<RigidbodyGravity>().SetGravityScale(m_RigidbodyGravity.GetGravityScale());
        m_Rigidbody.GetComponent<RigidbodyGravity>().SetRigidbodyDrag(m_RigidbodyGravity.GetRigidbodyDrag());

        Vector3 m_TrajectoryDir = (m_TrajectoryNext - m_TrajectoryStart) * GetTrajectoryPower();

        m_Rigidbody.velocity = m_TrajectoryDir;
    }

    public void SetBullet(Rigidbody2D m_Rigidbody2D, Vector2 m_TrajectoryStart, Vector2 m_TrajectoryNext)
    {
        //Set velocity to Bullet!

        Vector2 m_TrajectoryDir = (m_TrajectoryNext - m_TrajectoryStart) * GetTrajectoryPower();

        //m_Rigidbody2D.drag = m_RigidbodyGravity.GetRigidbodyDrag();
        m_Rigidbody2D.mass = 0;
        m_Rigidbody2D.gravityScale = m_RigidbodyGravity.GetGravityScale();
        m_Rigidbody2D.velocity = m_TrajectoryDir;
    }

    #endregion

    #region Line Renderer with Trajectory

    public void SetTrajectoryLineRenderer(LineRenderer comLineRenderer, float m_RigidbodyDrag, bool m_Dir_Normalized)
    {
        Vector3[] trajectory = GetTrajectoryPoints(m_RigidbodyDrag, m_Dir_Normalized);

        comLineRenderer.positionCount = trajectory.Length;
        Vector3[] position = new Vector3[trajectory.Length];

        for (int i = 0; i < position.Length; i++)
        {
            position[i] = trajectory[i];
        }

        comLineRenderer.SetPositions(position);
    }

    public void SetTrajectoryLineRendererClear(LineRenderer comLineRenderer)
    {
        comLineRenderer.positionCount = 0;
        comLineRenderer.SetPositions(new Vector3[0]);
    }

    #endregion
}