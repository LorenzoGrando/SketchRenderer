using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SketchRenderer.Runtime.Data;
using SketchRenderer.Runtime.Rendering.RendererFeatures;
using SketchRenderer.Runtime.Rendering.Volume;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SketchVolumeOverrider : MonoBehaviour
{
    [SerializeField]
    private UniversalRendererData rendererData;
    [SerializeField]
    private Volume volume;
    
    //Volume overrides
    private RenderUVsVolumeComponent uvsComponent;
    private SmoothOutlineVolumeComponent smoothOutlineComponent;
    private SketchOutlineVolumeComponent sketchOutlineComponent;
    private MaterialVolumeComponent materialComponent;
    private LuminanceVolumeComponent luminanceComponent;
    private CompositionVolumeComponent compositionComponent;
    
    [Serializable]
    internal struct SketchPreset
    {
        public SketchRendererContext Context;
        public string PresetName;
    }
    [SerializeField]
    private SketchPreset[] presets;
    
    private bool initialized = false;

    private void Start()
    {
        StartCoroutine(AwaitInit());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            ApplyPreset(1);
    }

    private void Initialize()
    {
        if (initialized)
            return;
        
        if (VolumeManager.instance == null || VolumeManager.instance.stack == null)
            return;
        
        volume = GetComponent<Volume>();
        
        volume.profile.TryGet<RenderUVsVolumeComponent>(out uvsComponent);
        volume.profile.TryGet<SmoothOutlineVolumeComponent>(out smoothOutlineComponent);
        volume.profile.TryGet<SketchOutlineVolumeComponent>(out sketchOutlineComponent);
        volume.profile.TryGet<MaterialVolumeComponent>(out materialComponent);
        volume.profile.TryGet<LuminanceVolumeComponent>(out luminanceComponent);
        volume.profile.TryGet<CompositionVolumeComponent>(out compositionComponent);
        initialized = true;
    }

    private IEnumerator AwaitInit()
    {
        while (!initialized)
        {
            Initialize();
            yield return null;
        }
        
        ApplyPreset(0);
    }

    public void CopyFromContext(SketchRendererContext context, List<SketchRendererFeatureType> features = null)
    {
        bool hasFilterList = features != null;

        if (!hasFilterList || (hasFilterList && features.Contains(SketchRendererFeatureType.UVS)))
        {
            uvsComponent.CopyFromContext(context);
            uvsComponent.active = context.IsFeaturePresent(SketchRendererFeatureType.UVS);
            uvsComponent.SetAllOverridesTo(uvsComponent.active);
        }
        if (!hasFilterList || (hasFilterList && features.Contains(SketchRendererFeatureType.OUTLINE_SMOOTH)))
        {
            smoothOutlineComponent.CopyFromContext(context);
            smoothOutlineComponent.active = context.IsFeaturePresent(SketchRendererFeatureType.OUTLINE_SMOOTH);
            smoothOutlineComponent.SetAllOverridesTo(smoothOutlineComponent.active);
        }
        if (!hasFilterList || (hasFilterList && features.Contains(SketchRendererFeatureType.OUTLINE_SKETCH)))
        {
            sketchOutlineComponent.CopyFromContext(context);
            sketchOutlineComponent.active = context.IsFeaturePresent(SketchRendererFeatureType.OUTLINE_SKETCH);
            sketchOutlineComponent.SetAllOverridesTo(sketchOutlineComponent.active);
        }
        if (!hasFilterList || (hasFilterList && features.Contains(SketchRendererFeatureType.MATERIAL)))
        {
            materialComponent.CopyFromContext(context);
            materialComponent.active = context.IsFeaturePresent(SketchRendererFeatureType.MATERIAL);
            materialComponent.SetAllOverridesTo(materialComponent.active);
        }
        if (!hasFilterList || (hasFilterList && features.Contains(SketchRendererFeatureType.LUMINANCE)))
        {
            luminanceComponent.CopyFromContext(context);
            luminanceComponent.active = context.IsFeaturePresent(SketchRendererFeatureType.LUMINANCE);
            luminanceComponent.SetAllOverridesTo(luminanceComponent.active);
        }
        if (!hasFilterList || (hasFilterList && features.Contains(SketchRendererFeatureType.COMPOSITOR)))
        {
            compositionComponent.CopyFromContext(context);
            compositionComponent.active = context.IsFeaturePresent(SketchRendererFeatureType.COMPOSITOR);
            compositionComponent.SetAllOverridesTo(compositionComponent.active);
        }
        
        UpdateActiveFeatures(context);
    }

    public void UpdateActiveFeatures(SketchRendererContext context)
    {
        List<SketchRendererFeatureType> features = new List<SketchRendererFeatureType>();
        int possibleFeatures = Enum.GetValues(typeof(SketchRendererFeatureType)).Length;
        for (int i = 0; i < possibleFeatures; i++)
        {
            SketchRendererFeatureType feat = (SketchRendererFeatureType)i;
            if(context.IsFeaturePresent(feat))
                features.Add(feat);
        }
        
        compositionComponent.SetRenderingTargets(features);
    }

    public void ApplyPreset(int presetIndex)
    {
        CopyFromContext(presets[presetIndex].Context);
    }
}