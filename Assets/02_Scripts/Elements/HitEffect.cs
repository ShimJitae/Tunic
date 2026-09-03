using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CinemachineImpulseSource))]
public class HitEffect : MonoBehaviour
{
    [Header("피격 색상")]
    [SerializeField] private Color hitColor = Color.orange;
    [SerializeField, Min(0f)] private float hitColorDuration = 0.1f;

    [Header("카메라 흔들림")]
    [SerializeField, Min(0f)] private float impulseCamera = 1f;

    private Health health;
    private CinemachineImpulseSource impulseSource;

    private readonly List<Material> materials = new();
    private readonly List<Color> originalColors = new();

    private int hitColorVersion;


    private void Awake()
    {
        health = GetComponent<Health>();
        impulseSource =
            GetComponent<CinemachineImpulseSource>();

        CacheMaterials();
    }

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;

        hitColorVersion++;

        RestoreOriginalColors();
    }

    private void HandleDamaged(float _)
    {
        PlayHitColorAsync().Forget();
        PlayCameraImpulse();
    }

    private void CacheMaterials()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer targetRenderer in renderers)
        {
            foreach (Material material in targetRenderer.materials)
            {
                if (material == null)
                    continue;

                if (!material.HasProperty("_BaseColor") &&
                    !material.HasProperty("_Color"))
                {
                    continue;
                }

                materials.Add(material);
                originalColors.Add(material.color);
            }
        }
    }

    private async UniTask PlayHitColorAsync()
    {
        int currentVersion = ++hitColorVersion;

        SetMaterialColors(hitColor);

        bool isCanceled = await UniTask
            .Delay(
                TimeSpan.FromSeconds(hitColorDuration),
                cancellationToken:
                    this.GetCancellationTokenOnDestroy())
            .SuppressCancellationThrow();

        if (isCanceled ||
            currentVersion != hitColorVersion)
        {
            return;
        }

        RestoreOriginalColors();
    }

    private void SetMaterialColors(Color color)
    {
        foreach (Material material in materials)
            material.color = color;
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < materials.Count; i++)
            materials[i].color = originalColors[i];
    }

    private void PlayCameraImpulse()
    {
        if (impulseCamera <= 0f)
            return;

        impulseSource.GenerateImpulseWithForce(
            impulseCamera);
    }
}