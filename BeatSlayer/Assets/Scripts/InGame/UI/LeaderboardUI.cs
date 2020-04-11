using LeaderboardManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    public Transform leaderboardContent;
    
    public void Show()
    {
        List<LeaderboardRecord> records = LeaderboardManager.GetLeaderboard();
    }
}