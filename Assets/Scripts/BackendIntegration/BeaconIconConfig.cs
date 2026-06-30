using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BeaconIconConfig",
    menuName = "CrowdSourcing/Beacon Icon Config")]
public class BeaconIconConfig : ScriptableObject
{
    [Serializable]
    public class CategoryIcon
    {
        public string category;

        public Texture2D icon;

        public Color color = Color.white;
    }

    [Header("Category Icon Mapping")]
    [SerializeField]
    private List<CategoryIcon> categoryIcons = new();

    [Header("Fallback")]
    [SerializeField]
    private Texture2D defaultIcon;

    private Dictionary<string, CategoryIcon> _lookup;

    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        _lookup = new Dictionary<string, CategoryIcon>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in categoryIcons)
        {
            if (string.IsNullOrWhiteSpace(item.category))
                continue;

            _lookup[item.category] = item;
        }
    }

    public Texture2D GetIcon(string category)
    {
        if (_lookup == null)
            BuildCache();

        if (!string.IsNullOrWhiteSpace(category) &&
            _lookup.TryGetValue(category, out CategoryIcon entry))
        {
            return entry.icon;
        }

        return defaultIcon;
    }

    public Color GetColor(string category)
    {
        if (_lookup == null)
            BuildCache();

        if (!string.IsNullOrWhiteSpace(category) &&
            _lookup.TryGetValue(category, out CategoryIcon entry))
        {
            return entry.color;
        }

        return Color.white;
    }
}