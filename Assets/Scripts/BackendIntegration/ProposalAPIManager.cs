using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VRCrowdSourcing.BackendIntegration
{
    public class ProposalApiManager : MonoBehaviour
    {
        [Header("API")]
        [SerializeField] private string apiUrl = "http://localhost:8899/api/datasets";

        [Header("Spawner")]
        [SerializeField] private ProposalMarkerSpawner markerSpawner;

        private void Awake()
        {
            if (markerSpawner == null)
            {
                markerSpawner = FindFirstObjectByType<ProposalMarkerSpawner>(FindObjectsInactive.Include);
            }
        }

        private void Start()
        {
            Debug.Log("ProposalApiManager Started");
            Debug.Log("ProposalApiManager API URL: " + apiUrl);
            LoadProposals(apiUrl);
        }

        public void LoadProposals(string requestUrl)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                Debug.LogError("ProposalApiManager.LoadProposals called without a valid URL.");
                return;
            }

            StartCoroutine(GetProposalData(requestUrl));
        }

        private IEnumerator GetProposalData(string requestUrl)
        {
            Debug.Log("API Request Started");

            using UnityWebRequest request = UnityWebRequest.Get(requestUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"DEBUG-API Error: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            ProposalResponse response = ParseProposalResponse(json);

            Debug.Log($"DEBUG-API Response: {json}");

            if (response?.proposals == null || response.proposals.Count == 0)
            {
                Debug.LogError("DEBUG-Invalid API response format - No proposal data found.");
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (markerSpawner == null)
            {
                markerSpawner = FindFirstObjectByType<ProposalMarkerSpawner>(FindObjectsInactive.Include);
            }

            if (markerSpawner == null)
            {
                Debug.LogError("ProposalMarkerSpawner NOT FOUND");
                yield break;
            }

            markerSpawner.SpawnMarkers(response.proposals);
        }

        private static ProposalResponse ParseProposalResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            string trimmedJson = json.Trim();

            if (trimmedJson.StartsWith('['))
            {
                ProposalResponse[] datasets = JsonUtility.FromJson<ProposalResponseArrayWrapper>("{\"items\":" + trimmedJson + "}").items;

                if (datasets == null || datasets.Length == 0)
                {
                    return null;
                }

                ProposalResponse response = new ProposalResponse
                {
                    proposals = new List<ProposalData>()
                };

                foreach (ProposalResponse dataset in datasets)
                {
                    if (dataset?.proposals == null)
                    {
                        continue;
                    }

                    response.proposals.AddRange(dataset.proposals);
                }

                return response;
            }

            return JsonUtility.FromJson<ProposalResponse>(trimmedJson);
        }

        [Serializable]
        private class ProposalResponseArrayWrapper
        {
            public ProposalResponse[] items = Array.Empty<ProposalResponse>();
        }
    }
}