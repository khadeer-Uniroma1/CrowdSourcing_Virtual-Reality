using System;
using System.Collections.Generic;

namespace VRCrowdSourcing.BackendIntegration
{
  [Serializable]
  public class ProposalResponse
  {
    public string city;
    public string generatedAt;
    public List<ProposalData> proposals;
  }

  [Serializable]
  public class ProposalData
  {
    public int id;
    public string title;
    public string description;
    public double latitude;
    public double longitude;
    public string category; // return type should be List<Categories> (e.g., "Infrastructure", "Public Safety", "Environment")
    public int votes; // return type should be List<Votes> (e.g., "Upvote", "Downvote")
    public string status; // return type should be List<Status> (e.g., "Open", "In Review", "Closed")
    public string severity; // return type should be List<Severity> (e.g., "Low", "Medium", "High")
    public string date; // should be DateTime type
    public List<string> images;
    public string video;
    public int cityDatasetId;
    public string city;
  }
}