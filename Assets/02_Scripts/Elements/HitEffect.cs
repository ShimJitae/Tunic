using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SFXPlayer))]
[RequireComponent(typeof(CinemachineImpulseSource))]
public class HitEffect : MonoBehaviour
{
    [Header("피격 색상")]
    [SerializeField] private Color hitColor = Color.orange;
    [SerializeField, Min(0f)] private float hitColorDuration = 0.1f;

    [Header("카메라 흔들림")]
    [SerializeField, Min(0f)] private float impulseCamera = 1f;

    [Header("피격 시 효과음")]
    [SerializeField] private SFXPlayer hitSFXPlayer;

    private Health health;
    private CinemachineImpulseSource impulseSource;

    private readonly List<Material> materials = new();
    private readonly List<Color> originalColors = new();

    private Tween hitColorTween;

    private void Awake()
    {
        health = GetComponent<Health>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (hitSFXPlayer == null || !gameObject.TryGetComponent(out hitSFXPlayer))
        {
            Debug.LogError($"HitEffect : {gameObject.name}에 SFXPlayer가 연결되지 않았습니다.");
        }

        CacheMaterials();
    }

    private void OnEnable()
    {
        health.OnDamaged += HandleDamaged;
        health.OnDamaged += hitSFXPlayer.Play;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        health.OnDamaged -= hitSFXPlayer.Play;

        hitColorTween?.Kill();
        RestoreOriginalColors();
    }

    private void HandleDamaged(float _)
    {
        PlayHitColor();
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

    private void PlayHitColor()
    {
        hitColorTween?.Kill();
        SetMaterialColors(hitColor);

        hitColorTween = DOVirtual.DelayedCall(
                hitColorDuration, RestoreOriginalColors, ignoreTimeScale: false)
            .OnKill(() => hitColorTween = null);
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
