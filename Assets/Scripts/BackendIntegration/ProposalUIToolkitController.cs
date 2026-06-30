using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using VRCrowdSourcing.BackendIntegration;

public class ProposalUIToolkitController : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement footer;

    private Label titleLabel;
    private Label voteBadge;
    private Label categoryLabel;
    private Label statusLabel;
    private Label severityLabel;
    private Label dateLabel;
    private Label descriptionLabel;
    private VisualElement proposalImage;
    private Button voteButton;
    private Button navigateButton;
    private Button closeButton;

    private ProposalData currentProposal;
    private UIFollowPlayer followPlayer;
    private const int MaxDescriptionLength = 400;

    public static ProposalUIToolkitController Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Ensure the document is loaded
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument reference is missing!");
            return;
        }

        root = uiDocument.rootVisualElement;
        footer = root.Q<VisualElement>("Footer");

        titleLabel = root.Q<Label>("TitleLabel");
        voteBadge = root.Q<Label>("VoteBadge");

        categoryLabel = root.Q<Label>("CategoryLabel");
        statusLabel = root.Q<Label>("StatusLabel");
        severityLabel = root.Q<Label>("SeverityLabel");
        dateLabel = root.Q<Label>("DateLabel");

        descriptionLabel = root.Q<Label>("DescriptionLabel");

        proposalImage = root.Q<VisualElement>("ProposalImage");
       // proposalImage.pickingMode = PickingMode.Ignore;

        voteButton = root.Q<Button>("VoteButton");
        navigateButton = root.Q<Button>("NavigateButton");
        closeButton = root.Q<Button>("CloseButton");

        voteButton?.RegisterCallback<UnityEngine.UIElements.PointerDownEvent>(evt => Debug.Log("VoteButton PointerDown"));
        voteButton?.RegisterCallback<UnityEngine.UIElements.PointerUpEvent>(evt => Debug.Log("VoteButton PointerUp"));
        voteButton?.RegisterCallback<UnityEngine.UIElements.PointerEnterEvent>(evt => Debug.Log("VoteButton PointerEnter"));
        voteButton?.RegisterCallback<UnityEngine.UIElements.ClickEvent>(evt => Debug.Log("VoteButton ClickEvent"));

        Debug.Log($"VoteButton found: {voteButton != null}");
        Debug.Log($"NavigateButton found: {navigateButton != null}");
        Debug.Log($"CloseButton found: {closeButton != null}");

        if (voteButton != null)
        {
            voteButton.clicked += OnVoteClicked;
        }
        else
        {
            Debug.LogError("Failed to find VoteButton! Check name in UXML.");
        }

        if (navigateButton != null)
        {
            navigateButton.clicked += OnNavigateClicked;
        }
        else
        {
            Debug.LogError("Failed to find NavigateButton! Check name in UXML.");
        }

        if (closeButton != null)
        {
            //closeButton.clicked += HidePanel;
            closeButton.RegisterCallback<ClickEvent>(evt => HidePanel());
        }
        else
        {
            Debug.LogError("Failed to find CloseButton! Check name in UXML.");
        }

        followPlayer = GetComponent<UIFollowPlayer>();
    }

    private void Start()
    {
        HidePanel();
    }

    public void SetProposalData(ProposalData proposal)
    {
        currentProposal = proposal;

        titleLabel.text = proposal.title;
        voteBadge.text = $"\U0001f525 {proposal.votes} Votes";
        categoryLabel.text = $"Category : {proposal.category}";
        statusLabel.text = $"Status : {proposal.status}";
        severityLabel.text = $"Severity : {proposal.severity}";
        dateLabel.text = $"Date : {proposal.date}";
        descriptionLabel.text = TruncateDescription(proposal.description);

        ShowPanel();

        if (proposal.images != null &&
            proposal.images.Count > 0)
        {
            StartCoroutine(LoadImage(proposal.images[0]));
        }
    }

    private IEnumerator LoadImage(string url)
    {
        using var request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            yield break;

        var texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);

        proposalImage.style.backgroundImage = new StyleBackground(texture);
    }

    private void ShowPanel()
    {
        if (root == null) return;
        root.style.display = DisplayStyle.Flex;

        // Force the UI to update layout and event system immediately
        //root.schedule.Execute(() => {}).ExecuteLater(0);
        root.schedule.Execute(() =>
            {
                Debug.Log($"Footer Bounds: {footer.worldBound}");
                Debug.Log($"Image Bounds: {proposalImage.worldBound}");

                Debug.Log($"Footer Height: {footer.resolvedStyle.height}");
                Debug.Log($"Image Height: {proposalImage.resolvedStyle.height}");
            }).ExecuteLater(100);

        Camera cam = Camera.main;

        if (followPlayer != null && cam != null)
        {
            followPlayer.StartFollowing(cam.transform);
        }
    }

    private void HidePanel()
    {
        if (root == null) return;
        root.style.display = DisplayStyle.None;

        if (followPlayer != null)
        {
            followPlayer.StopFollowing();
        }
        Debug.Log("Close Clicked");
    }

    private void OnVoteClicked()
    {
        if (currentProposal == null) return;

        currentProposal.votes++;

        if (voteBadge != null)
        {
            voteBadge.text = $"\U0001f525 {currentProposal.votes} Votes";
        }
    }

    private void OnNavigateClicked()
    {
        Debug.Log("Navigate Clicked");
    }

    private string TruncateDescription(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (text.Length <= MaxDescriptionLength)
            return text;

        return text.Substring(0, MaxDescriptionLength) + "...";
    }
}
