using QuickMethode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlJumpY2D : MonoBehaviour
{
    #region Varible: Jump

    [Header("Jump")]
    [SerializeField] private bool m_jumpHold = true;
    [SerializeField] [Min(0)] private float m_jumpForce = 10f;
    [SerializeField] [Min(0)] private float m_jumpRatio = 1f;
    [SerializeField] [Min(1)] private int m_jumpUpdate = 2;
    [SerializeField] [Min(0)] private int m_jumpUpdateMin = 0;

    private float JumpForceCurrent => m_jumpForce * m_jumpRatio;

    //Event
    private bool m_jumpContinue = false;
    private bool m_jumpUp = false;
    private bool m_jumpKeep = false;
    private bool m_jumpShort = false;

    private Coroutine m_iSetJumpContinue;

    #endregion

    #region Varible: Hold

    [Header("Down")]
    [SerializeField] [Min(0)] private float m_downStop = 1f;
    [SerializeField] [Min(0)] private float m_downForce = 1f;

    [Header("Gravity")]
    [SerializeField] [Min(0)] private float m_gravityScale = 1f;

    #endregion

    #region Varible: Public

    public float JumpForce { get => m_jumpForce; set => m_jumpForce = value; }

    public float JumpRatio { get => m_jumpRatio; set => m_jumpRatio = value; }

    public float GravityScale { get => m_gravityScale; set => m_gravityScale = value; }

    #endregion

    private Rigidbody2D m_rigidbody;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    #region Jump Progess

    public void SetProgessJump()
    {
        #region -------------------------------- Jump Press

        if (!m_jumpContinue)
        {
            //Gravity when Jump or on air!!
            float GravityForce = -Physics2D.gravity.y * m_gravityScale;
            m_rigidbody.AddForce(Vector2.down * GravityForce, ForceMode2D.Force);
        }

        if (m_jumpContinue && m_jumpUp)
        {
            //Jump Up start!!
            float ForceUp = Mathf.Sqrt(-Physics2D.gravity.y * JumpForceCurrent) * m_rigidbody.mass;
            m_rigidbody.AddForce(Vector2.up * ForceUp, ForceMode2D.Impulse);
        }

        if (!m_jumpUp)
            //Jump Up continue!!
            return;

        if (m_rigidbody.velocity.y < 0)
            //Jump Up end!!
            m_jumpUp = false;

        #endregion

        #region -------------------------------- Jump Hold

        if (!m_jumpHold)
            //Jump Hold optional!!
            return;

        if (m_jumpShort)
            return;

        if (m_rigidbody.velocity.y > 0 && !m_jumpKeep)
        {
            //Drag Down in middle!!
            float ForceDown = Mathf.Sqrt(-Physics2D.gravity.y * JumpForceCurrent * m_downStop) * m_rigidbody.mass;
            m_rigidbody.AddForce(Vector2.down * m_rigidbody.velocity.y * ForceDown, ForceMode2D.Force);
        }
        else
        if (m_rigidbody.velocity.y < 0)
        {
            //Drag Down at begin!!
            float ForceDown = Mathf.Sqrt(-Physics2D.gravity.y * JumpForceCurrent * m_downForce) * m_rigidbody.mass;
            m_rigidbody.AddForce(Vector2.down * m_rigidbody.velocity.y * ForceDown, ForceMode2D.Force);
        }

        #endregion
    } //Fixed Update!!

    public void SetEventClick()
    {
        m_jumpUp = true;

        if (m_iSetJumpContinue != null)
            StopCoroutine(m_iSetJumpContinue);
        m_iSetJumpContinue = StartCoroutine(ISetJumpContinue());

        if (m_jumpUpdateMin > 0)
            StartCoroutine(ISetJumpShort());
    } //Event Update!!

    private IEnumerator ISetJumpContinue()
    {
        m_jumpContinue = true;

        for (int Fixed = 0; Fixed < m_jumpUpdate; Fixed++)
            yield return new WaitForFixedUpdate();

        m_jumpContinue = false;
    }

    private IEnumerator ISetJumpShort()
    {
        m_jumpShort = true;

        for (int Fixed = 0; Fixed < m_jumpUpdateMin; Fixed++)
            yield return new WaitForFixedUpdate();

        m_jumpShort = false;
    }

    public void SetEventHold()
    {
        m_jumpKeep = true;
    } //Event Update!!

    public void SetEventRelease()
    {
        m_jumpKeep = false;
    } //Event Update!!

    #endregion
}
