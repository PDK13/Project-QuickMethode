using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { private set; get; }

    #region Varible: Setting

    [SerializeField] private StringConfig m_stringConfig;

    #endregion

    #region Varible: Debug

    private enum DebugType { None = 0, Primary = 1, Full = int.MaxValue, }

    [Space]
    [SerializeField] private DebugType m_debug = DebugType.None;

    #endregion

    #region Event

    /// <summary>
    /// Message system stage current active
    /// </summary>
    public Action<MessageStageType> onStageActive;

    /// <summary>
    /// Message system current author and trigger active
    /// </summary>
    public Action<MessageDataConfigTextAuthor, MessageDataConfigTextTrigger> onTextActive;

    /// <summary>
    /// Message system current choice check
    /// </summary>
    public Action<int, MessageDataConfigChoice> onChoiceCheck;

    /// <summary>
    /// Message system current choice active
    /// </summary>
    public Action<int, MessageDataConfigChoice> onChoiceActive;

    #endregion

    #region Varible: Message Manager

    private enum MessageCommandType
    {
        None,
        Text,
        Done,
        Wait,
        Next,
        Skip,
        Choice,
    }

    private MessageCommandType m_command = MessageCommandType.Text;
    [SerializeField] private MessageDataConfig m_data;

    [SerializeField] private TextMeshProUGUI m_tmp;
    [SerializeField] private string m_current = "";

    private Coroutine m_iSetMessageShowSingle;

    public List<MessageDataConfigChoice> ChoiceList => m_data.Choice;

    [SerializeField] private bool m_active = false;
    [SerializeField] private bool m_choice = false;

    [SerializeField] private MessageStageType m_stage = MessageStageType.None;

    /// <summary>
    /// Message system stage current
    /// </summary>
    public MessageStageType Stage => m_stage;

    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //
        Instance = this;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    #region Main

    /// <summary>
    /// Start message with config data
    /// </summary>
    /// <param name="TextMessPro"></param>
    /// <param name="MessageData"></param>
    public void SetStart(TextMeshProUGUI TextMessPro, MessageDataConfig MessageData)
    {
        if (m_active)
            return;
        //
        m_data = MessageData;
        m_tmp = TextMessPro;
        //
        StartCoroutine(ISetMessageShow(false));
    }

    private IEnumerator ISetMessageShow(bool WaitForNextMessage)
    {
        if (WaitForNextMessage)
            //Not check when first show message!!
            yield return new WaitUntil(() => m_command == MessageCommandType.Next);
        //
        m_command = MessageCommandType.None;
        m_active = true;
        m_choice = false;
        //
        SetStage(MessageStageType.Start);
        //
        if ((int)m_debug > (int)DebugType.None)
            Debug.Log("[Message] Start!");
        //
        for (int i = 0; i < m_data.Message.Count; i++)
        {
            MessageDataConfigText MessageSingle = m_data.Message[i];
            m_current = MessageSingle.Message;
            //
            //MESSAGE:
            if (m_current == null)
                m_tmp.text = "...";
            else
            if (m_current == "")
                m_tmp.text = "...";
            else
            {
                //BEGIN:
                onTextActive?.Invoke(m_data.Message[i].Author, m_data.Message[i].Trigger);
                //
                m_tmp.text = "";
                //
                if (m_stringConfig != null)
                    m_current = m_stringConfig.GetColorHexFormatReplace(m_current);
                //
                //PROGESS:
                m_command = MessageCommandType.Text;
                //
                SetStage(MessageStageType.Text);
                //
                if ((int)m_debug > (int)DebugType.None)
                    Debug.Log("[Message] " + m_current);
                //
                m_iSetMessageShowSingle = StartCoroutine(ISetMessageShowSingle(MessageSingle));
                yield return new WaitUntil(() => m_command == MessageCommandType.Skip || m_command == MessageCommandType.Done);
                //
                //DONE:
                m_tmp.text = m_current;
            }
            //WAIT:
            if (m_current != "" && i < m_data.Message.Count - 1)
            {
                //FINAL:
                m_command = MessageCommandType.Wait;
                //
                SetStage(MessageStageType.Wait);
                //
                if ((int)m_debug > (int)DebugType.None)
                    Debug.Log("[Message] Next?");
                //
                yield return new WaitUntil(() => m_command == MessageCommandType.Next);
            }
        }
        //
        m_command = m_data.ChoiceAvaible ? MessageCommandType.Choice : MessageCommandType.None;
        m_active = m_data.ChoiceAvaible;
        m_choice = m_data.ChoiceAvaible;
        //
        SetStage(m_choice ? MessageStageType.Choice : MessageStageType.End);
        //
        if (m_choice)
        {
            if ((int)m_debug > (int)DebugType.None)
                Debug.Log("[Message] Choice?");
        }
        else
        {
            if ((int)m_debug > (int)DebugType.None)
                Debug.Log("[Message] End!");
        }
    }

    private IEnumerator ISetMessageShowSingle(MessageDataConfigText MessageSingle)
    {
        bool HtmlFormat = false;
        //
        foreach (char MessageChar in m_current)
        {
            //TEXT:
            m_tmp.text += MessageChar;
            //
            //COLOR:
            if (!HtmlFormat && MessageChar == '<')
            {
                HtmlFormat = true;
                continue;
            }
            else
            if (HtmlFormat && MessageChar == '>')
            {
                HtmlFormat = false;
                continue;
            }
            //
            //DELAY:
            if (HtmlFormat)
                continue;
            //
            switch (MessageChar)
            {
                case '.':
                case '?':
                case '!':
                case ':':
                    if (MessageSingle.Delay.Mark > 0)
                        yield return new WaitForSeconds(MessageSingle.Delay.Mark);
                    break;
                case ' ':
                    if (MessageSingle.Delay.Space > 0)
                        yield return new WaitForSeconds(MessageSingle.Delay.Space);
                    break;
                default:
                    if (MessageSingle.Delay.Alpha > 0)
                        yield return new WaitForSeconds(MessageSingle.Delay.Alpha);
                    break;
            }
        }
        //
        m_command = MessageCommandType.Done;
    }

    private void SetStage(MessageStageType Stage)
    {
        m_stage = Stage;
        onStageActive?.Invoke(Stage);
    }

    #endregion

    #region Control

    /// <summary>
    /// Next message; or continue message after choice option delay continue message
    /// </summary>
    public void SetNext()
    {
        if (m_command != MessageCommandType.Wait)
            //When current message in done show up, press Next to move on next message!
            return;
        //
        m_command = MessageCommandType.Next;
        //
        if ((int)m_debug > (int)DebugType.None)
            Debug.Log("[Message] Next!");
    }

    /// <summary>
    /// Skip current message, until got choice option or end message
    /// </summary>
    public void SetSkip()
    {
        if (m_command != MessageCommandType.Text)
            //When current message is showing up, press Next to skip and show full message!
            return;
        //
        StopCoroutine(m_iSetMessageShowSingle);
        //
        m_command = MessageCommandType.Skip;
        //
        if ((int)m_debug > (int)DebugType.None)
            Debug.Log("[Message] Skip!");
    }

    /// <summary>
    /// Check choice option of message when avaible
    /// </summary>
    /// <param name="ChoiceIndex"></param>
    public void SetChoiceCheck(int ChoiceIndex)
    {
        if (m_command != MessageCommandType.Choice)
            //When current message in done show up and got choice option, move choice option to get imformation of choice!
            return;
        //
        if (ChoiceIndex < 0 || ChoiceIndex > m_data.Choice.Count - 1)
            return;
        //
        onChoiceCheck?.Invoke(ChoiceIndex, m_data.Choice[ChoiceIndex]);
    }

    /// <summary>
    /// Choice option of message when avaible
    /// </summary>
    /// <param name="ChoiceIndex"></param>
    /// <param name="NextMessage">If false, must call 'Next' methode for continue message of last option choice</param>
    public void SetChoiceActive(int ChoiceIndex, bool NextMessage = true)
    {
        if (m_command != MessageCommandType.Choice)
            //When current message in done show up and got choice option, press choice option to move on next message!
            return;
        //
        if (ChoiceIndex < 0 || ChoiceIndex > m_data.Choice.Count - 1)
            return;
        //
        m_command = NextMessage ? MessageCommandType.Next : MessageCommandType.Wait;
        m_data = m_data.Choice[ChoiceIndex].Next;
        //
        onChoiceActive?.Invoke(ChoiceIndex, m_data.Choice[ChoiceIndex]);
        //
        StartCoroutine(ISetMessageShow(true));
        //
        if ((int)m_debug > (int)DebugType.None)
            Debug.LogFormat("[Message] Choice {0}: {1}", ChoiceIndex, m_data.Choice[ChoiceIndex].Text);
    }

    /// <summary>
    /// Stop message
    /// </summary>
    public void SetStop()
    {
        StopAllCoroutines();
        StopCoroutine(m_iSetMessageShowSingle);
        //
        m_command = MessageCommandType.None;
        m_active = false;
        m_choice = false;
        //
        SetStage(MessageStageType.End);
        //
        m_tmp.text = "";
        //
        if ((int)m_debug > (int)DebugType.None)
            Debug.LogFormat("[Message] Stop!");
    }

    #endregion
}

public enum MessageStageType
{
    None,
    //Trigger when Start
    Start,
    //Trigger when Show
    Text,
    Wait,
    Choice,
    //Trigger when End
    End,
}

#if UNITY_EDITOR

[CustomEditor(typeof(MessageManager))]
public class MessageManagerEditor : Editor
{
    private MessageManager Target;

    private SerializedProperty m_stringConfig;

    private SerializedProperty m_debug;
    private SerializedProperty m_data;
    private SerializedProperty m_tmp;
    private SerializedProperty m_current;
    private SerializedProperty m_stage;

    private void OnEnable()
    {
        Target = target as MessageManager;
        //
        m_stringConfig = QEditorCustom.GetField(this, "m_stringConfig");
        //
        m_debug = QEditorCustom.GetField(this, "m_debug");
        m_data = QEditorCustom.GetField(this, "m_data");
        m_tmp = QEditorCustom.GetField(this, "m_tmp");
        m_current = QEditorCustom.GetField(this, "m_current");
        m_stage = QEditorCustom.GetField(this, "m_stage");
    }

    public override void OnInspectorGUI()
    {
        QEditorCustom.SetUpdate(this);
        //
        QEditorCustom.SetField(m_stringConfig);
        //
        QEditorCustom.SetField(m_debug);
        //
        QEditor.SetDisableGroupBegin();
        //
        QEditorCustom.SetField(m_data);
        QEditorCustom.SetField(m_tmp);
        QEditorCustom.SetField(m_current);
        QEditorCustom.SetField(m_stage);
        //
        QEditor.SetDisableGroupEnd();
        //
        QEditorCustom.SetApply(this);
    }
}

#endif