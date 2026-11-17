using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Dimensions
{
    public float l;
    public float w;
    public float h;
}

[Serializable]
public class MeshEntry
{
    public string path;
    public string object_type;
    public string name;
    public string text_description;
    public Dimensions standardized_dimensions;
    public string status;
}

[Serializable]
public class MeshDatabase
{
    public List<MeshEntry> meshes;
}

public class GLBSelector : MonoBehaviour
{
    [Header("JSON")]
    public string jsonFileName = "backblaze.json";

    [Header("Backblaze")]
    public string bucketName = "previewar";
    public string endpoint = "s3.us-west-004.backblazeb2.com";

    [Header("Matching")]
    public bool allowRotationMatch = true;
    public bool onlyUseStatusOk = true;

    [Tooltip("If true, object_type can start with the requested option instead of requiring exact equality.")]
    public bool usePrefixTypeMatch = true;

    private MeshDatabase cachedDatabase;

    private void Awake()
    {
        cachedDatabase = LoadMeshDatabase();

        if (cachedDatabase != null && cachedDatabase.meshes != null)
        {
            Debug.Log($"Loaded mesh database with {cachedDatabase.meshes.Count} entries.");
        }
        else
        {
            Debug.LogError("Failed to load mesh database.");
        }
    }

    public MeshEntry GetRandomValidMesh(string objectTypePrefix, float availableL, float availableW, float availableH)
    {
        List<MeshEntry> validMeshes = GetValidMeshes(objectTypePrefix, availableL, availableW, availableH);

        if (validMeshes.Count == 0)
        {
            Debug.LogWarning(
                $"No valid GLBs found for object type prefix '{objectTypePrefix}' " +
                $"within dimensions ({availableL}, {availableW}, {availableH})."
            );
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, validMeshes.Count);
        MeshEntry chosen = validMeshes[randomIndex];

        Debug.Log($"Random valid mesh chosen: {chosen.name} | key={chosen.path} | type={chosen.object_type}");
        return chosen;
    }

    public List<MeshEntry> GetValidMeshes(string objectTypePrefix, float availableL, float availableW, float availableH)
    {
        if (cachedDatabase == null || cachedDatabase.meshes == null)
        {
            Debug.LogError("Mesh database is not loaded.");
            return new List<MeshEntry>();
        }

        List<MeshEntry> valid = new List<MeshEntry>();

        foreach (MeshEntry mesh in cachedDatabase.meshes)
        {
            if (mesh == null || mesh.standardized_dimensions == null)
                continue;

            if (onlyUseStatusOk && !string.Equals(mesh.status, "ok", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!MatchesObjectType(mesh.object_type, objectTypePrefix))
                continue;

            if (mesh.standardized_dimensions.l <= 0 ||
                mesh.standardized_dimensions.w <= 0 ||
                mesh.standardized_dimensions.h <= 0)
            {
                continue;
            }

            bool fits = FitsWithinExactOrder(mesh.standardized_dimensions, availableL, availableW, availableH);

            if (fits)
            {
                valid.Add(mesh);
            }
        }

        Debug.Log(
            $"GetValidMeshes -> objectTypePrefix={objectTypePrefix}, " +
            $"available=({availableL}, {availableW}, {availableH}), count={valid.Count}"
        );

        return valid;
    }

    private bool MatchesObjectType(string meshObjectType, string requestedType)
    {
        string meshType = meshObjectType?.Trim();
        string requested = requestedType?.Trim();

        if (string.IsNullOrWhiteSpace(meshType) || string.IsNullOrWhiteSpace(requested))
            return false;

        if (usePrefixTypeMatch)
            return meshType.StartsWith(requested, StringComparison.OrdinalIgnoreCase);

        return string.Equals(meshType, requested, StringComparison.OrdinalIgnoreCase);
    }

    public string BuildBackblazeUrl(MeshEntry mesh)
    {
        if (mesh == null || string.IsNullOrWhiteSpace(mesh.path))
            return null;

        return BuildBackblazeUrlFromObjectKey(mesh.path);
    }

    public string BuildBackblazeUrlFromObjectKey(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return null;

        string cleanedKey = objectKey.Trim().Replace("\\", "/");
        return $"https://{bucketName}.{endpoint}/{cleanedKey}";
    }

    private MeshDatabase LoadMeshDatabase()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        Debug.Log($"Loading mesh database from: {fullPath}");
        Debug.Log($"File exists: {File.Exists(fullPath)}");

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"JSON file does not exist: {fullPath}");
            return null;
        }

        string json = File.ReadAllText(fullPath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("JSON file is empty.");
            return null;
        }

        MeshDatabase db = JsonUtility.FromJson<MeshDatabase>(json);

        if (db == null || db.meshes == null)
        {
            Debug.LogError("Failed to parse mesh database JSON.");
            return null;
        }

        return db;
    }

    private bool FitsWithinExactOrder(Dimensions dims, float availableL, float availableW, float availableH)
    {
        float objectLength = dims.l;
        float objectWidth = dims.w;
        float objectHeight = dims.h;

        return objectLength <= availableL &&
            objectWidth  <= availableW &&
            objectHeight <= availableH;
    }

    private bool FitsWithinAnyOrder(Dimensions dims, float availableL, float availableW, float availableH)
    {
        float objectLength = dims.l;
        float objectWidth = dims.h;   // swapped: JSON h is actually width
        float objectHeight = dims.w;  // swapped: JSON w is actually height

        // Height must stay vertical.
        bool heightFits = objectHeight <= availableH;

        // No floor-plane rotation.
        bool fitsWithoutRotation =
            objectLength <= availableL &&
            objectWidth <= availableW;

        // Floor-plane rotation only: length and width can swap.
        bool fitsWithFloorRotation =
            objectWidth <= availableL &&
            objectLength <= availableW;

        return heightFits && (fitsWithoutRotation || fitsWithFloorRotation);
    }
}