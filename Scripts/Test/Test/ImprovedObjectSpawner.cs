using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ImprovedObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject prefab;
        public float spawnTime;
        public Sprite danceStepSprite;
    }

    [Header("Spawn Settings")]
    public List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
    public Transform spawnPoint;
    public float introDelay = 5f;

    [Header("UI Settings")]
    public GameObject danceStepPrefab;
    public RectTransform goalPosition;
    public float moveDuration = 2f;
    public GameObject hitEffectPrefab;
    public Canvas worldSpaceCanvas;
    public RectTransform panel;
    public Image goalImage; // Reference to the goal image for visual feedback

    [Header("Object Pooling")]
    public int poolSize = 20;
    public int danceStepPoolSize = 20;
    public int hitEffectPoolSize = 10;

    // Object pools
    private Dictionary<GameObject, Queue<GameObject>> objectPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Queue<GameObject> danceStepPool = new Queue<GameObject>();
    private Queue<GameObject> hitEffectPool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePools();
    }

    private void Start()
    {
        // Sort spawn infos by time for better processing
        spawnInfos.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
        StartCoroutine(SpawnObjectsSequence());
    }

    private void InitializePools()
    {
        // Create pools for each unique prefab
        HashSet<GameObject> uniquePrefabs = new HashSet<GameObject>();
        foreach (var info in spawnInfos)
        {
            if (info.prefab != null && !uniquePrefabs.Contains(info.prefab))
            {
                uniquePrefabs.Add(info.prefab);
                CreatePool(info.prefab, poolSize);
            }
        }

        // Create dance step pool
        for (int i = 0; i < danceStepPoolSize; i++)
        {
            GameObject danceStep = Instantiate(danceStepPrefab, panel);
            danceStep.SetActive(false);
            danceStepPool.Enqueue(danceStep);
        }

        // Create hit effect pool
        if (hitEffectPrefab != null)
        {
            for (int i = 0; i < hitEffectPoolSize; i++)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab);
                hitEffect.SetActive(false);
                hitEffectPool.Enqueue(hitEffect);
            }
        }
    }

    private void CreatePool(GameObject prefab, int size)
    {
        Queue<GameObject> pool = new Queue<GameObject>();
        
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        objectPools[prefab] = pool;
    }

    private GameObject GetPooledObject(GameObject prefab)
    {
        if (!objectPools.ContainsKey(prefab))
        {
            Debug.LogWarning($"No pool found for prefab: {prefab.name}. Creating one now.");
            CreatePool(prefab, poolSize);
        }

        Queue<GameObject> pool = objectPools[prefab];
        
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            pool.Enqueue(obj); // Cycle it back to the end of the queue
            return obj;
        }
        else
        {
            // Pool is empty, instantiate a new object
            Debug.LogWarning($"Pool for {prefab.name} is empty. Consider increasing pool size.");
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            return obj;
        }
    }

    private GameObject GetDanceStepFromPool()
    {
        if (danceStepPool.Count > 0)
        {
            GameObject danceStep = danceStepPool.Dequeue();
            danceStepPool.Enqueue(danceStep);
            return danceStep;
        }
        else
        {
            GameObject danceStep = Instantiate(danceStepPrefab, panel);
            danceStep.SetActive(false);
            return danceStep;
        }
    }

    private GameObject GetHitEffectFromPool()
    {
        if (hitEffectPool.Count > 0)
        {
            GameObject hitEffect = hitEffectPool.Dequeue();
            hitEffectPool.Enqueue(hitEffect);
            return hitEffect;
        }
        else if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab);
            hitEffect.SetActive(false);
            return hitEffect;
        }
        return null;
    }

    private IEnumerator SpawnObjectsSequence()
    {
        // Wait for the intro delay
        yield return new WaitForSeconds(introDelay);

        float startTime = Time.time;
        int index = 0;

        // Process all spawn infos in order
        while (index < spawnInfos.Count)
        {
            SpawnInfo currentInfo = spawnInfos[index];
            float currentTime = Time.time - startTime;

            if (currentTime >= currentInfo.spawnTime)
            {
                SpawnObject(currentInfo, index);
                index++;
            }
            
            yield return null;
        }
    }

    private void SpawnObject(SpawnInfo info, int index)
    {
        if (info.prefab == null)
        {
            Debug.LogWarning($"Prefab at index {index} is null!");
            return;
        }

        // Get and position the object from pool
        GameObject obj = GetPooledObject(info.prefab);
        obj.transform.position = spawnPoint.position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        // Spawn dance step if sprite exists
        if (info.danceStepSprite != null)
        {
            SpawnDanceStep(info.danceStepSprite);
        }
    }

    private void SpawnDanceStep(Sprite sprite)
    {
        GameObject danceStep = GetDanceStepFromPool();
        Image danceStepImage = danceStep.GetComponent<Image>();
        RectTransform danceStepRectTransform = danceStep.GetComponent<RectTransform>();

        if (danceStepImage == null || danceStepRectTransform == null)
        {
            Debug.LogError("Dance step prefab is missing required components!");
            return;
        }

        // Configure the dance step
        danceStepImage.sprite = sprite;
        danceStepRectTransform.position = danceStepPrefab.transform.position;
        danceStepRectTransform.localScale = Vector3.one;
        danceStep.SetActive(true);

        // Move the dance step
        StartCoroutine(MoveDanceStep(danceStepRectTransform));
    }

    private IEnumerator MoveDanceStep(RectTransform danceStepRectTransform)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = danceStepRectTransform.position;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            // Move the image from start to goal
            danceStepRectTransform.position = Vector3.Lerp(startPosition, goalPosition.position, t);
            yield return null;
        }

        // Trigger effect when reaching the goal
        TriggerHitEffect(danceStepRectTransform.position);
        danceStepRectTransform.gameObject.SetActive(false);
    }

    private void TriggerHitEffect(Vector3 position)
    {
        // Use pooled hit effect
        GameObject hitEffect = GetHitEffectFromPool();
        if (hitEffect != null)
        {
            hitEffect.transform.position = position;
            hitEffect.SetActive(true);

            // Automatically deactivate after a delay
            StartCoroutine(DeactivateAfterDelay(hitEffect, 1f));
        }

        // Make the goal give visual feedback
        StartCoroutine(PulseGoalImage());
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    private IEnumerator PulseGoalImage()
    {
        if (goalImage == null)
        {
            yield break;
        }

        // Save original color
        Color originalColor = goalImage.color;
        Color glowColor = Color.yellow;

        // Pulse animation
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Pulse in and out
            float pulse = Mathf.PingPong(t * 2, 1);
            goalImage.color = Color.Lerp(originalColor, glowColor, pulse);
            
            yield return null;
        }

        // Reset color
        goalImage.color = originalColor;
    }
}