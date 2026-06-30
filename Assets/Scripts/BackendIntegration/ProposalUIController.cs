using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace VRCrowdSourcing.BackendIntegration
{
    public class ProposalUIController : MonoBehaviour
    {
        public static ProposalUIController Instance { get; private set; }

        [Header("Panel Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Header")]
        [SerializeField] private TMP_Text titleText;

        [Header("Metadata")]
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text votesText;
        [SerializeField] private TMP_Text severityText;
        [SerializeField] private TMP_Text dateText;

        [Header("Description")]
        [SerializeField] private TMP_Text descriptionText;

        [Header("Media")]
        [SerializeField] private Image image;

        [Header("Buttons")]
        [SerializeField] private Button voteButton;
        [SerializeField] private Button navigateButton;
        [SerializeField] private Button closeButton;

        private ProposalData currentProposal;

        private UIFollowPlayer followPlayer;

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

            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            followPlayer = panelRoot.GetComponent<UIFollowPlayer>();
            Debug.Log("Follow Player Component = " + followPlayer);

            ValidateReferences();
            HidePanel();
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(HidePanel);

            if (voteButton != null) voteButton.onClick.AddListener(OnVoteClicked);

            if (navigateButton != null) navigateButton.onClick.AddListener(OnNavigateClicked);
        }

        private void OnDisable()
        {
            if (closeButton != null)  closeButton.onClick.RemoveListener(HidePanel);

            if (voteButton != null)   voteButton.onClick.RemoveListener(OnVoteClicked);

            if (navigateButton != null)   navigateButton.onClick.RemoveListener(OnNavigateClicked);
        }

        public void SetProposalData(ProposalData proposal)
        {
            if (proposal == null)
            {
                Debug.LogError("Proposal data is NULL");
                return;
            }

            currentProposal = proposal;

            if (panelRoot != null)
            {
                ShowPanel();
            }

            if (titleText != null) titleText.text = proposal.title;
            if (categoryText != null) categoryText.text = "Category: " + proposal.category;
            if (statusText != null) statusText.text = "Status: " + proposal.status;
            if (votesText != null) votesText.text = "Votes: " + proposal.votes;
            if (severityText != null) severityText.text = "Severity: " + proposal.severity;
            if (dateText != null) dateText.text = "Date: " + proposal.date;
            if (descriptionText != null) descriptionText.text = proposal.description;

            if (image != null)
            {
                image.sprite = null;
                if (proposal.images != null &&
                    proposal.images.Count > 0 &&
                    !string.IsNullOrEmpty(proposal.images[0]))
                {
                    StartCoroutine(
                        LoadImage(proposal.images[0]));
                }
            }
        }

        private IEnumerator LoadImage(string imageUrl)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            Debug.Log("Loading image from URL: " + imageUrl);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load image: " + request.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            image.sprite = sprite;
            image.preserveAspect = true;
            image.SetNativeSize();
            image.color = Color.white;

            Debug.Log($"Sprite assigned: {sprite.name}");
            Debug.Log($"Image size: {texture.width} x {texture.height}");
            Debug.Log($"UI Image component: {image}");
            Debug.Log("Image loaded successfully");
        }

        public void ShowPanel()
        {
            Camera targetCamera = null;

            if (XRReferences.Instance != null && XRReferences.Instance.mainCamera != null)
            {
                targetCamera = XRReferences.Instance.mainCamera;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }

            if (targetCamera == null)
            {
                Debug.LogError("No camera available to position the proposal panel.");
                return;
            }

            panelRoot.SetActive(true);

            if (followPlayer != null)
            {
                followPlayer.StartFollowing(targetCamera.transform);
            }
        }

        public void HidePanel()
        {
            if (followPlayer != null)
            {
                followPlayer.StopFollowing();
            }
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnVoteClicked()
        {
            if (currentProposal == null)
            {
                return;
            }

            Debug.Log("Vote clicked for: " + currentProposal.title);

            currentProposal.votes++;

            if (votesText != null)
            {
                votesText.text = "Votes: " + currentProposal.votes;
            }
        }

        private static void OnNavigateClicked()
        {
            Debug.Log("Navigate clicked");

            // Later:
            // teleport player
            // create route
            // move drone
        }

        private void ValidateReferences()
        {
            if (panelRoot == null) Debug.LogError("Panel Root missing");
            if (titleText == null) Debug.LogError("TitleText missing");
            if (categoryText == null) Debug.LogError("CategoryText missing");
            if (statusText == null) Debug.LogError("StatusText missing");
            if (votesText == null) Debug.LogError("VotesText missing");
            if (severityText == null) Debug.LogError("SeverityText missing");
            if (dateText == null) Debug.LogError("DateText missing");
            if (descriptionText == null) Debug.LogError("DescriptionText missing");
            if (image == null) Debug.LogError("image missing");
            if (voteButton == null) Debug.LogError("VoteButton missing");
            if (navigateButton == null) Debug.LogError("NavigateButton missing");
            if (closeButton == null) Debug.LogError("CloseButton missing");
        }
    }
}