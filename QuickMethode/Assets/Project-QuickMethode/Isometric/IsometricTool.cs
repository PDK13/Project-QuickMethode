#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IsometricTool : EditorWindow
{
    #region Enum

    private enum MainType { World, Block, }

    #endregion

    #region Varible: Manager

    private IsometricManager m_manager;
    private bool m_managerUpdate = false;

    private string m_pathOpen = "";
    private string m_pathSave = "";

    #endregion

    #region Varible: Curson

    private IsometricBlock m_curson;
    private IsometricBlock m_focus;
    private Event m_event;

    private bool m_check = false; //When turn ON, always focus on Curson!
    private bool m_camera = false; //When turn ON, main Camera always follow Curson!

    private bool m_maskXY = false;
    private bool m_hiddenH = false;

    #endregion

    #region Varible: Main

    private MainType m_main;

    #endregion

    #region Varible: World Manager

    private int m_indexTag = 0;
    private int m_indexName = 0;
    private int m_countNameHorizontal = 4;

    private List<string> m_listTag;
    private int m_indexTagLast = 0;

    #endregion

    #region Varible: Scroll

    private Vector2 m_scrollTag;
    private Vector2 m_scrollBlock;

    #endregion

    [MenuItem("Tools/IsometricTool")]
    public static void Init()
    {
        GetWindow<IsometricTool>("IsometricTool");
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            return;
        //
        if (GetManager())
            SetManager(true);
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            QUnityEditor.SetLabel("(Not avaible when playing)", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this));
            return;
        }
        //
        if (GetManager())
            SetManager(true);
        else
            return;
        //
        QUnityEditor.SetLabel("TOOL MANAGER", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this));
        //
        SetGUIGroupManager();
        //
        QUnityEditor.SetSpace(5f);
        QUnityEditor.SetLabel("CURSON MANAGER", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this));
        //
        SetCursonControl();
        //
        SetGUIGroupCurson();
        //
        if (m_manager.List.BlockList.Count == 0)
            return;
        //
        QUnityEditor.SetSpace(5f);
        QUnityEditor.SetLabel("MAIN MANAGER", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this));
        //
        //if (QUnityEditor.SetButton((m_main == MainType.World ? "WORLD" : "BLOCK"), null, QUnityEditorWindow.GetGUILayoutWidth(this)))
        //    m_main = m_main == MainType.World ? MainType.Block : MainType.World;
        //
        switch (m_main)
        {
            case MainType.World:
                QUnityEditor.SetSpace(5f);
                SetGUIGroupTag();
                QUnityEditor.SetSpace(5f);
                SetGUIGroupBlock();
                break;
            case MainType.Block:
                QUnityEditor.SetSpace(5f);
                //...
                break;
        }
    }

    #region Manager

    private bool GetManager()
    {
        if (m_manager == null)
            m_manager = GameObject.FindFirstObjectByType<IsometricManager>();
        //
        return m_manager != null;
    }

    private void SetManager(bool Force)
    {
        if (m_manager == null)
            return;
        //
        if (!Force)
        {
            if (m_managerUpdate)
                return;
            m_managerUpdate = true;
        }
        //
        m_manager.SetInit();
        m_manager.List.SetList(m_manager.IsometricConfig, false);
        m_manager.World.SetWorldRead(m_manager.transform);
        //
        m_listTag = new List<string>();
        for (int i = 0; i < m_manager.List.BlockList.Count; i++)
            m_listTag.Add(m_manager.List.BlockList[i].Tag);
    }

    #endregion

    #region Curson

    //GUI Curson

    private void SetGUIGroupCurson()
    {
        if (m_curson == null)
        {
            return;
        }

        QUnityEditor.SetHorizontalBegin();
        {
            QUnityEditor.SetBackground(Color.white);
            QUnityEditor.SetLabel("RENDERER: ", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
            m_manager.Game.Scene.Renderer = (IsometricRendererType)QUnityEditor.SetPopup<IsometricRendererType>((int)m_manager.Game.Scene.Renderer, QUnityEditorWindow.GetGUILayoutWidth(this, 0.75f, 2.5f));
        }
        QUnityEditor.SetHorizontalEnd();

        QUnityEditor.SetHorizontalBegin();
        {
            QUnityEditor.SetBackground(Color.white);
            QUnityEditor.SetLabel("ROTATE: ", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
            QUnityEditor.SetChanceCheckBegin();
            m_manager.Game.Scene.Rotate = (IsometricRotateType)QUnityEditor.SetPopup<IsometricRotateType>((int)m_manager.Game.Scene.Rotate, QUnityEditorWindow.GetGUILayoutWidth(this, 0.75f, 2.5f));
            if (QUnityEditor.SetChanceCheckEnd())
            {
                m_manager.Game.Scene.Centre = m_curson.Pos;
                m_manager.Game.Scene.Centre.H = 0;
            }
        }
        QUnityEditor.SetHorizontalEnd();

        QUnityEditor.SetHorizontalBegin();
        {
            QUnityEditor.SetBackground(Color.white);
            QUnityEditor.SetLabel("CURSON: ", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
            QUnityEditor.SetLabel(m_curson.Pos.XInt.ToString(), QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
            QUnityEditor.SetLabel(m_curson.Pos.YInt.ToString(), QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
            QUnityEditor.SetLabel(m_curson.Pos.HInt.ToString(), QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
        }
        QUnityEditor.SetHorizontalEnd();

        if (m_focus != null)
        {
            QUnityEditor.SetHorizontalBegin();
            {
                QUnityEditor.SetBackground(Color.white);
                QUnityEditor.SetLabel("FOCUS: ", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
                QUnityEditor.SetLabel(m_focus.Pos.XInt.ToString(), QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
                QUnityEditor.SetLabel(m_focus.Pos.YInt.ToString(), QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
                QUnityEditor.SetLabel(m_focus.Pos.HInt.ToString(), QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 0.25f));
            }
            QUnityEditor.SetHorizontalEnd();
        }

        if (m_manager.List.BlockList.Count > 0)
        {
            QUnityEditor.SetHorizontalBegin();
            SetGUIButtonFocus(0.25f);
            SetGUIButtonBack(0.25f);
            SetGUIButtonCheck(0.25f);
            SetGUIButtonCamera(0.25f);
            QUnityEditor.SetHorizontalEnd();
            QUnityEditor.SetHorizontalBegin();
            SetGUIButtonMaskXY(0.5f);
            SetGUIButtonMaskH(0.5f);
            QUnityEditor.SetHorizontalEnd();
        }
    }

    private void SetGUIButtonFocus(float WidthPercent)
    {
        if (QUnityEditor.SetButton("FOCUS", QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, WidthPercent)))
        {
            IsometricBlock BlockFocus = m_manager.World.GetBlockPrimary(m_curson.Pos);
            if (BlockFocus != null)
            {
                QGameObject.SetFocus(BlockFocus.gameObject);
                m_focus = BlockFocus;
            }
        }
    }

    private void SetGUIButtonBack(float WidthPercent)
    {
        if (QUnityEditor.SetButton("BACK", QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, WidthPercent)))
        {
            if (m_focus != null)
            {
                m_curson.Pos = m_focus.Pos;
            }
        }
    }

    private void SetGUIButtonCheck(float WidthPercent)
    {
        if (QUnityEditor.SetButton("CHECK", QUnityEditor.GetGUIButton(m_check ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, WidthPercent)))
        {
            m_check = !m_check;
            SetCursonCheck();
        }
    }

    private void SetGUIButtonCamera(float WidthPercent)
    {
        if (QUnityEditor.SetButton("CAMERA", QUnityEditor.GetGUIButton(m_camera ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, WidthPercent)))
        {
            m_camera = !m_camera;
        }
    }

    private void SetGUIButtonMaskXY(float WidthPercent)
    {
        if (QUnityEditor.SetButton("XY", QUnityEditor.GetGUIButton(m_maskXY ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, WidthPercent)))
        {
            m_maskXY = !m_maskXY;
            SetCursonMaskXY();
            SetCursonHiddenH();
        }
    }

    private void SetGUIButtonMaskH(float WidthPercent)
    {
        if (QUnityEditor.SetButton("H", QUnityEditor.GetGUIButton(m_hiddenH ? FontStyle.Bold : FontStyle.Normal, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, WidthPercent)))
        {
            m_hiddenH = !m_hiddenH;
            SetCursonMaskXY();
            SetCursonHiddenH();
        }
    }

    //GUI Curson Control

    private void SetCursonControl()
    {
        if (m_curson == null)
        {
            Transform Curson = m_manager.transform.Find(IsometricDataWorld.CURSON_NAME);

            if (Curson == null)
            {
                GameObject CursonClone = QGameObject.SetCreate(IsometricDataWorld.CURSON_NAME, m_manager.transform);
                m_curson = CursonClone.AddComponent<IsometricBlock>();
            }
            else
            if (Curson.GetComponent<IsometricBlock>() == null)
            {
                m_curson = Curson.gameObject.AddComponent<IsometricBlock>();
            }
            else
            {
                m_curson = Curson.GetComponent<IsometricBlock>();
            }

            if (m_curson.GetComponent<SpriteRenderer>() == null)
            {
                m_curson.gameObject.AddComponent<SpriteRenderer>();
            }

            m_curson.WorldManager = m_manager;
        }

        if (m_curson != null)
        {
            if (m_manager.List.BlockList.Count > 0)
            {
                m_curson.GetComponent<SpriteRenderer>().sprite = m_manager.List.BlockList[m_indexTag].Block[m_indexName].GetComponent<SpriteRenderer>().sprite;
                m_curson.GetComponent<SpriteRenderer>().color = m_manager.List.BlockList[m_indexTag].Block[m_indexName].GetComponent<SpriteRenderer>().color;
            }
        }

        m_event = Event.current;

        switch (m_event.type)
        {
            case EventType.KeyDown:
                {
                    switch (Event.current.keyCode)
                    {
                        //Move Curson!!
                        case KeyCode.UpArrow:
                            m_curson.Pos += IsometricVector.GetRotateDir(IsometricVector.GetDir(IsoDir.Up), m_manager.Game.Scene.Rotate);
                            SetCursonMaskXY();
                            SetCursonHiddenH();
                            SetCursonCheck();
                            SetCameraFollow();
                            m_event.Use();
                            break;
                        case KeyCode.DownArrow:
                            m_curson.Pos += IsometricVector.GetRotateDir(IsometricVector.GetDir(IsoDir.Down), m_manager.Game.Scene.Rotate);
                            SetCursonMaskXY();
                            SetCursonHiddenH();
                            SetCursonCheck();
                            SetCameraFollow();
                            m_event.Use();
                            break;
                        case KeyCode.LeftArrow:
                            m_curson.Pos += IsometricVector.GetRotateDir(IsometricVector.GetDir(IsoDir.Left), m_manager.Game.Scene.Rotate);
                            SetCursonMaskXY();
                            SetCursonHiddenH();
                            SetCursonCheck();
                            SetCameraFollow();
                            m_event.Use();
                            break;
                        case KeyCode.RightArrow:
                            m_curson.Pos += IsometricVector.GetRotateDir(IsometricVector.GetDir(IsoDir.Right), m_manager.Game.Scene.Rotate);
                            SetCursonMaskXY();
                            SetCursonHiddenH();
                            SetCursonCheck();
                            SetCameraFollow();
                            m_event.Use();
                            break;
                        case KeyCode.PageUp:
                            m_curson.Pos += IsometricVector.GetRotateDir(IsometricVector.GetDir(IsoDir.Top), m_manager.Game.Scene.Rotate);
                            SetCursonMaskXY();
                            SetCursonHiddenH();
                            SetCursonCheck();
                            SetCameraFollow();
                            m_event.Use();
                            break;
                        case KeyCode.PageDown:
                            m_curson.Pos += IsometricVector.GetRotateDir(IsometricVector.GetDir(IsoDir.Bot), m_manager.Game.Scene.Rotate);
                            SetCursonMaskXY();
                            SetCursonHiddenH();
                            SetCursonCheck();
                            SetCameraFollow();
                            m_event.Use();
                            break;
                        //Block Curson!!
                        case KeyCode.Home:
                            m_manager.World.SetBlockCreate(m_curson.Pos, m_manager.List.BlockList[m_indexTag].Block[m_indexName].gameObject);
                            m_event.Use();
                            break;
                        case KeyCode.End:
                            m_manager.World.SetBlockRemovePrimary(m_curson.Pos);
                            m_event.Use();
                            break;
                    }
                }
                break;
        }
        //Event Keyboard when Tool on focus!!
    }

    private void SetCursonCheck()
    {
        if (m_check)
        {
            IsometricBlock BlockFocus = m_manager.World.GetBlockPrimary(m_curson.Pos);
            if (BlockFocus != null)
            {
                QGameObject.SetFocus(BlockFocus.gameObject);
            }
        }
    }

    private void SetCameraFollow()
    {
        if (m_camera)
        {
            Camera.main.transform.position = m_curson.transform.position + Vector3.back * 100f;
        }
    }

    private void SetCursonMaskXY()
    {
        if (m_maskXY)
        {
            bool CentreFound = m_manager.World.SetEditorMask(m_curson.Pos, Color.red, Color.white, Color.yellow);
            m_curson.GetComponent<SpriteRenderer>().enabled = !CentreFound;
        }
        else
        {
            bool CentreFound = m_manager.World.SetEditorMask(m_curson.Pos, Color.white, Color.white, Color.white);
            m_curson.GetComponent<SpriteRenderer>().enabled = !CentreFound;
        }
    }

    private void SetCursonHiddenH()
    {
        if (m_hiddenH)
        {
            m_manager.World.SetEditorHidden(m_curson.Pos.HInt, 0.01f);
        }
        else
        {
            m_manager.World.SetEditorHidden(m_curson.Pos.HInt, 1f);
        }
    }

    #endregion

    #region Manager

    private void SetGUIGroupManager()
    {
        QUnityEditor.SetHorizontalBegin();
        if (QUnityEditor.SetButton("Refresh", QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 1f / 2)))
        {
            m_maskXY = false;
            m_hiddenH = false;
            m_indexTag = 0;
            m_indexName = 0;
            //
            if (GetManager())
                SetManager(true);
            //
            SetCursonMaskXY();
            SetCursonHiddenH();
            //
            QUnityAssets.SetRefresh();
        }
        if (QUnityEditor.SetButton("Clear", QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 1f / 2)))
        {
            m_manager.World.SetWorldRemove();
        }
        QUnityEditor.SetHorizontalEnd();
        QUnityEditor.SetHorizontalBegin();
        if (QUnityEditor.SetButton("Save", QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 1f / 2)))
        {
            (bool Result, string Path, string Name) Path = QPath.GetPathFileSavePanel("Save", "txt", m_pathSave == "" ? QPath.GetPath(QPath.PathType.Assets) : m_pathSave);
            if (Path.Result)
            {
                m_pathSave = Path.Path;
                IsometricDataFile.SetFileWrite(m_manager, QPath.GetPath(QPath.PathType.None, Path.Path));
                QUnityAssets.SetRefresh();
            }
        }
        if (QUnityEditor.SetButton("Open", QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 1f / 2)))
        {
            QUnityAssets.SetRefresh();
            //
            (bool Result, string Path, string Name) Path = QPath.GetPathFileOpenPanel("Open", "txt", m_pathOpen == "" ? QPath.GetPath(QPath.PathType.Assets) : m_pathOpen);
            if (Path.Result)
            {
                m_maskXY = false;
                m_hiddenH = false;
                m_indexTag = 0;
                m_indexName = 0;
                //
                m_manager.List.SetList(m_manager.IsometricConfig, false);
                //
                m_pathOpen = Path.Path;
                IsometricDataFile.SetFileRead(m_manager, QPath.GetPath(QPath.PathType.None, Path.Path));
            }
        }
        QUnityEditor.SetHorizontalEnd();
    }

    #endregion

    #region World Manager

    //Group Tag

    private void SetGUIGroupTag()
    {
        QUnityEditor.SetLabel("TAG", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this));
        //
        //SetGUIGroupTagButton();
        //
        SetGUIGroupTagPopup();
    }

    private void SetGUIGroupTagButton()
    {
        m_scrollTag = QUnityEditor.SetScrollViewBegin(m_scrollTag, QUnityEditor.GetGUIHeight(100));
        for (int i = 0; i < m_manager.List.BlockList.Count; i++)
        {
            string Tag = m_manager.List.BlockList[i].Tag != "" ? m_manager.List.BlockList[i].Tag : "[...]";
            if (m_indexTag == i)
            {
                if (QUnityEditor.SetButton(Tag, QUnityEditor.GetGUIButton(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 1f, 0f)))
                {
                    m_indexTag = i;
                    m_indexName = 0;
                }
            }
            else
            {
                if (QUnityEditor.SetButton(Tag, QUnityEditor.GetGUIButton(FontStyle.Normal, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this, 1f, 0f)))
                {
                    m_indexTag = i;
                    m_indexName = 0;
                }
            }
        }
        QUnityEditor.SetScrollViewEnd();
    }

    private void SetGUIGroupTagPopup()
    {
        m_indexTag = QUnityEditor.SetPopup(m_indexTag, m_listTag);
        if (m_indexTag != m_indexTagLast)
        {
            m_indexTagLast = m_indexTag;
            m_indexName = 0;
        }
    }

    //Group Block

    private void SetGUIGroupBlock()
    {
        QUnityEditor.SetLabel("BLOCK", QUnityEditor.GetGUILabel(FontStyle.Bold, TextAnchor.MiddleCenter), QUnityEditorWindow.GetGUILayoutWidth(this));
        {
            QUnityEditor.SetHorizontalBegin();
            if (QUnityEditor.SetButton(" - ", null, QUnityEditor.GetGUIWidth(20f)))
            {
                if (m_countNameHorizontal > 1)
                {
                    m_countNameHorizontal--;
                }
            }
            QUnityEditor.SetLabel(m_countNameHorizontal.ToString(), QUnityEditor.GetGUILabel(FontStyle.Normal, TextAnchor.MiddleCenter), QUnityEditor.GetGUIWidth(20));
            if (QUnityEditor.SetButton(" + ", null, QUnityEditor.GetGUIWidth(20f)))
            {
                m_countNameHorizontal++;
            }
            QUnityEditor.SetHorizontalEnd();
        }
        m_scrollBlock = QUnityEditor.SetScrollViewBegin(m_scrollBlock);
        int BlockIndex = 0;
        while (BlockIndex <= m_manager.List.BlockList[m_indexTag].Block.Count - 1)
        {
            QUnityEditor.SetHorizontalBegin();
            for (int i = 0; i < m_countNameHorizontal; i++)
            {
                if (BlockIndex > m_manager.List.BlockList[m_indexTag].Block.Count - 1)
                    continue;
                //
                QUnityEditor.SetBackground(m_indexName == BlockIndex ? Color.white : Color.clear);
                if (GetGUIGroupBlockButton(BlockIndex))
                    m_indexName = BlockIndex;
                //
                BlockIndex++;
            }
            QUnityEditor.SetHorizontalEnd();
        }
        QUnityEditor.SetScrollViewEnd();
    }

    private bool GetGUIGroupBlockButton(int Index)
    {
        return QUnityEditor.SetButton(
            m_manager.List.BlockList[m_indexTag].Block[Index].GetComponent<SpriteRenderer>().sprite,
            QUnityEditorWindow.GetGUILayoutWidth(this, 1f / m_countNameHorizontal),
            QUnityEditorWindow.GetGUILayoutHeightBaseWidth(this, 1f / m_countNameHorizontal));
    }

    #endregion

    #region Block Manager

    //...

    #endregion
}

#endif