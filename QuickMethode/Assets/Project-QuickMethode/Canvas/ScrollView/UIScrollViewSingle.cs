using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIScrollViewSingle : MonoBehaviour
{
    [Header("Setting")]

    [SerializeField] private ScrollViewType m_ScrollViewType;

    [SerializeField] private Vector2 m_ItemSize = new Vector2(100f, 100f);

    [SerializeField] private Vector2 m_ItemSpacing = new Vector2(5f, 5f);

    [SerializeField] [Min(1)] private int m_ItemConstraint = 1;

    [Header("Testing")]

    [SerializeField] [Range(0, 10)] float m_IndexTesting = 0;

    [SerializeField] bool m_CheckTesting = false;

    [Header("Component")]

    [SerializeField] private ScrollRect com_ScrollRect;

    [SerializeField] private RectTransform com_Content;

    [SerializeField] private ContentSizeFitter com_ContentSizeFitter;

    [SerializeField] private GridLayoutGroup com_GridLayoutGroup;

    private void Awake()
    {
        if (this.GetComponent<ScrollRect>() == null)
        {
            Debug.LogWarningFormat("{0}: Script not attach to Scroll View GameObject?!", name);
        }
        else
        {
            //Scroll Rect

            com_ScrollRect = this.GetComponent<ScrollRect>();
        }

        if (this.transform.Find("Viewport/Content") == null)
        {
            Debug.LogWarningFormat("{0}: Something went wrong to get child Viewport/Content of Scroll View!", name);

            return;
        }
        else
        {
            com_Content = this.transform.Find("Viewport/Content").GetComponent<RectTransform>();

            //Grid Layout Group

            if (com_Content.gameObject.GetComponent<ContentSizeFitter>() == null)
            {
                com_ContentSizeFitter = com_Content.gameObject.AddComponent<ContentSizeFitter>();
            }

            if (com_ContentSizeFitter == null)
            {
                com_ContentSizeFitter = com_Content.gameObject.GetComponent<ContentSizeFitter>();
            }

            //Content Size Fitter

            if (com_Content.gameObject.GetComponent<GridLayoutGroup>() == null)
            {
                com_GridLayoutGroup = com_Content.gameObject.AddComponent<GridLayoutGroup>();
            }

            if (com_GridLayoutGroup == null)
            {
                com_GridLayoutGroup = com_Content.gameObject.GetComponent<GridLayoutGroup>();
            }
        }

        SetScrollViewFix();
    }

    private void Start()
    {
        if (Application.IsPlaying(this.gameObject))
        {
            SetCheckTestingOff();
        }
    }

#if UNITY_EDITOR

    private void Update()
    {
        SetScrollViewFix();

        if (m_CheckTesting)
        {
            SetContent((float)m_IndexTesting);
        }
    }

#endif

    #region ================================================ Content Primary

    public void SetScrollViewTouch(bool ScrollViewTouch)
    {
        com_ScrollRect.enabled = ScrollViewTouch;
    }

    private void SetScrollViewFix()
    {
        switch (m_ScrollViewType)
        {
            case ScrollViewType.Vertical:
                //Content
                if (com_ScrollRect != null)
                {
                    com_Content.anchoredPosition = new Vector2(0, com_Content.anchoredPosition.y);
                }
                //Scroll Rect
                if (com_ContentSizeFitter != null)
                {
                    com_ScrollRect.vertical = true;
                    com_ScrollRect.horizontal = false;
                }
                //Grid Layout Group
                if (com_GridLayoutGroup != null)
                {
                    com_GridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    com_GridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    com_GridLayoutGroup.constraintCount = m_ItemConstraint;
                }
                //Content Size Fitter
                if (com_GridLayoutGroup == null)
                {
                    com_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    com_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
                break;
            case ScrollViewType.Horizontal:
                if (com_ScrollRect != null)
                {
                    com_Content.anchoredPosition = new Vector2(com_Content.anchoredPosition.x, 0);
                }
                //Scroll Rect
                if (com_ContentSizeFitter != null)
                {
                    com_ScrollRect.vertical = false;
                    com_ScrollRect.horizontal = true;
                }
                //Grid Layout Group
                if (com_GridLayoutGroup != null)
                {
                    com_GridLayoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    com_GridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    com_GridLayoutGroup.constraintCount = m_ItemConstraint;
                }
                //Content Size Fitter
                if (com_GridLayoutGroup != null)
                {
                    com_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    com_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
                break;
        }

        if (com_GridLayoutGroup != null)
        {
            com_GridLayoutGroup.cellSize = m_ItemSize;
            com_GridLayoutGroup.spacing = m_ItemSpacing;
        }
    }

    #endregion

    #region ================================================ Content Pos

    #region Content Pos Free

    public void SetContent(float ItemIndex)
    {
        if (ItemIndex < 0)
            com_Content.anchoredPosition = GetContent(0);
        else
        if (ItemIndex > com_Content.childCount - 1)
            com_Content.anchoredPosition = GetContent(com_Content.childCount - 1);
        else
            com_Content.anchoredPosition = GetContent(ItemIndex);
    }

    public Vector2 GetContent(float ItemIndex)
    {
        switch (m_ScrollViewType)
        {
            case ScrollViewType.Vertical:
                return (+1) * (ItemIndex / m_ItemConstraint) * new Vector2(0, m_ItemSize.y + m_ItemSpacing.y);
            case ScrollViewType.Horizontal:
                return (-1) * (ItemIndex / m_ItemConstraint) * new Vector2(m_ItemSize.x + m_ItemSpacing.x, 0);
        }
        return new Vector2();
    }

    #endregion

    #region Content Pos Fixed

    public void SetContent(int ItemIndex)
    {
        if (ItemIndex < 0)
            com_Content.anchoredPosition = GetContent(0);
        else
        if (ItemIndex > com_Content.childCount - 1)
            com_Content.anchoredPosition = GetContent(com_Content.childCount - 1);
        else
            com_Content.anchoredPosition = GetContent(ItemIndex);
    }

    public Vector2 GetContent(int ItemIndex)
    {
        switch (m_ScrollViewType)
        {
            case ScrollViewType.Vertical:
                return (+1) * (ItemIndex / m_ItemConstraint) * new Vector2(0, m_ItemSize.y + m_ItemSpacing.y);
            case ScrollViewType.Horizontal:
                return (-1) * (ItemIndex / m_ItemConstraint) * new Vector2(m_ItemSize.x + m_ItemSpacing.x, 0);
        }
        return new Vector2();
    }

    public Vector2 ContentLast => GetContent(ContentCount - 1);

    #endregion

    #endregion

    #region ================================================ Content RecTransform

    public RectTransform Content => com_Content;

    /// <summary>
    /// Use "Content" instead!!
    /// </summary>
    /// <returns></returns>
    public RectTransform GetContent()
    {
        return com_Content;
    }

    public int ContentCount => com_Content.childCount;

    public RectTransform GetContentItem(int ItemIndex)
    {
        if (ItemIndex < 0)
            return com_Content.GetChild(0).GetComponent<RectTransform>();

        if (ItemIndex > com_Content.childCount - 1)
            return com_Content.GetChild(com_Content.childCount - 1).GetComponent<RectTransform>();

        return com_Content.GetChild(ItemIndex).GetComponent<RectTransform>();
    }

    public List<RectTransform> GetContentItem()
    {
        List<RectTransform> ItemContent = new List<RectTransform>();
        for (int i = 0; i < ContentCount; i++)
            ItemContent.Add(Content.GetChild(i).GetComponent<RectTransform>());
        return ItemContent;
    }

    #endregion

    public void SetCheckTestingOff()
    {
        m_CheckTesting = false;
    }
}

public enum ScrollViewType
{
    Vertical,
    Horizontal,
}