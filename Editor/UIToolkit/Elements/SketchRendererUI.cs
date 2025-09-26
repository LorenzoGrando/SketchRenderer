using System;
using System.Drawing;
using SketchRenderer.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using FontStyle = UnityEngine.FontStyle;
using Object = UnityEngine.Object;

namespace SketchRenderer.Editor.UIToolkit
{
    public static class SketchRendererUI
    {
        internal static VisualElement SketchMajorArea(string name, bool applyMargins = true, float fontSize = -1f)
        {
            VisualElement container = new VisualElement();
            container.style.borderTopColor = SketchRendererUIData.BorderColor;
            container.style.borderTopWidth = SketchRendererUIData.MajorBorderWidth;
            
            if (!applyMargins)
                SketchRendererUIUtils.ApplyToMargins(container, CornerData.Empty);
            
            var label = new Label(name);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
          
            if (fontSize >= 0)
            {
                label.style.height = fontSize;
                label.style.fontSize = fontSize;
            }
          
            SketchRendererUIUtils.AddWithMargins(container, label, SketchRendererUIData.TitleIndent);
            return container;
        }

        internal static Button SketchMajorButton(string name, Action clickEvent, bool applyMargins = true)
        {
            Button button = new Button(clickEvent) { text = name };
            button.style.height = SketchRendererUIData.MajorButtonHeight;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            if(!applyMargins)
                SketchRendererUIUtils.ApplyToMargins(button, CornerData.Empty);
            
            return button;
        }

        internal static HelpBox SketchInmutableAssetHelpBox()
        {
            HelpBox helpBox = new HelpBox();
            helpBox.text = $"The assigned asset is inmutable.\nCreate and assign a new asset instance to change settings.";
            helpBox.style.justifyContent = Justify.Center;
            helpBox.style.unityTextAlign = TextAnchor.MiddleCenter;
            return helpBox;
        }
        
        internal static Foldout SketchFoldout(string name, bool applyMargins = true, bool applyPadding = true)
        {
            Foldout foldout = new Foldout();
            foldout.text = name;
            foldout.userData = applyMargins;
            
            foldout.RegisterCallbackOnce<GeometryChangedEvent>(evt => ApplyFoldoutStyle(evt, applyPadding));
            
            static void ApplyFoldoutStyle(GeometryChangedEvent evt, bool applyPadding)
            {
                Foldout foldout = (Foldout)evt.target;
                bool applyMargins = (bool)foldout.userData;
                
                var foldoutToggle = foldout.Q<Toggle>(className: Foldout.toggleUssClassName);
                foldoutToggle.style.borderTopColor = SketchRendererUIData.BorderColor;
                foldoutToggle.style.borderTopWidth = SketchRendererUIData.BorderWidth;
                foldoutToggle.style.backgroundColor = SketchRendererUIData.ExpandableHeaderBackgroundColor;
                
                foldoutToggle.style.minHeight = foldoutToggle.resolvedStyle.height * SketchRendererUIData.ExpandableHeaderHeightModifier;
              
                if(applyPadding)
                    SketchRendererUIUtils.ApplyToPadding(foldoutToggle, SketchRendererUIData.MajorIndentCorners);
                else
                    SketchRendererUIUtils.ApplyToPadding(foldoutToggle, CornerData.Empty);

                if (!applyMargins)
                    SketchRendererUIUtils.ApplyToMargins(foldoutToggle, CornerData.Empty);
                
                var foldoutLabel = foldout.Q<Label>(className: Foldout.textUssClassName);
                foldoutLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                foldoutLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                SketchRendererUIUtils.ApplyToMargins(foldoutLabel, CornerData.Empty);
                
                foldout.style.unityTextAlign = TextAnchor.MiddleLeft;
            }
            
            return foldout;
        }

        internal static Foldout SketchFoldoutWithToggle(string name, SerializedProperty toggleableFeature, bool applyMargins = true, bool applyPadding = true, EventCallback<ChangeEvent<bool>> changeCallback = null)
        {
            Foldout foldout = new Foldout();
            foldout.text = name;
            foldout.userData = applyMargins;
            
            Toggle featureToggle = new Toggle();
            featureToggle.BindProperty(toggleableFeature);
            featureToggle.TrackPropertyValue(toggleableFeature);
            
            foldout.RegisterCallbackOnce<GeometryChangedEvent>(evt => ApplyFoldoutStyle(evt, featureToggle, applyPadding, changeCallback));
            
            static void ApplyFoldoutStyle(GeometryChangedEvent evt, Toggle featureToggle, bool applyPadding, EventCallback<ChangeEvent<bool>> changeCallback = null)
            {
                Foldout foldout = (Foldout)evt.target;
                bool applyMargins = (bool)foldout.userData;
                
                var foldoutToggle = foldout.Q<Toggle>(className: Foldout.toggleUssClassName);
                foldoutToggle.style.borderTopColor = SketchRendererUIData.BorderColor;
                foldoutToggle.style.borderTopWidth = SketchRendererUIData.BorderWidth;
                foldoutToggle.style.backgroundColor = SketchRendererUIData.ExpandableHeaderBackgroundColor;
                
                foldoutToggle.style.minHeight = foldoutToggle.resolvedStyle.height * SketchRendererUIData.ExpandableHeaderHeightModifier;
              
                if(applyPadding)
                    SketchRendererUIUtils.ApplyToPadding(foldoutToggle, SketchRendererUIData.MajorIndentCorners);
                else
                    SketchRendererUIUtils.ApplyToPadding(foldoutToggle, CornerData.Empty);

                if (!applyMargins)
                    SketchRendererUIUtils.ApplyToMargins(foldoutToggle, CornerData.Empty);
                
                var foldoutLabel = foldout.Q<Label>(className: Foldout.textUssClassName);
                foldoutLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                foldoutLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                SketchRendererUIUtils.ApplyToMargins(foldoutLabel, CornerData.Empty);
                
                foldoutLabel.hierarchy.parent.Add(featureToggle);
                foldoutLabel.hierarchy.parent.style.justifyContent = Justify.SpaceBetween;
                
                featureToggle.RegisterValueChangedCallback(evt => ToggleFoldoutFeatures(evt, foldout, changeCallback));

                static void ToggleFoldoutFeatures(ChangeEvent<bool> evt, Foldout foldout, EventCallback<ChangeEvent<bool>> callback = null)
                {
                    foldout.ToggleElementInteractions(evt.newValue);
                    if(callback != null)
                        callback(evt);
                }
                
                foldout.style.unityTextAlign = TextAnchor.MiddleLeft;
            }
            
            return foldout;
        }

        internal static void ConfigureLabeledElement<T>(SketchElement<T> element, string labelName, VisualElement input) where T : BindableElement
        {
            element.Container.style.flexDirection = FlexDirection.Row;
            element.Container.style.justifyContent = Justify.SpaceBetween;
            
            element.Label = new Label(labelName);
            element.Label.style.minWidth = SketchRendererUIData.MinLabelWidth;
            element.Label.style.maxWidth = SketchRendererUIData.MaxLabelWidth;
            element.Label.style.overflow = Overflow.Hidden;
            element.Label.style.flexGrow = 1;
            element.Container.Add(element.Label );
            
            input.style.flexGrow = 1f;
            var preexistingLabel = input.Q<Label>();
            if (preexistingLabel != null)
            {
                bool foundLabel = false;
                foreach (var child in input.Children())
                {
                    if (child == preexistingLabel)
                    {
                        foundLabel = true;
                        break;
                    }
                }
                if(foundLabel)
                    input.Remove(preexistingLabel);
            }

            input.style.minWidth = SketchRendererUIData.LabeledInputAreaMinWidth;
            input.style.maxWidth = SketchRendererUIData.LabeledInputAreaMaxWidth;
            SketchRendererUIUtils.AddWithMargins(element.Container, input, CornerData.Empty);
        }

        private static void ConfigureSketchElementField<T>(SketchElement<T> element, VisualElement field, string name) where T : BindableElement
        {
            element.Container = new VisualElement();
            element.Container.userData = name;
            element.Container.RegisterCallbackOnce<GeometryChangedEvent>(evt => ApplyElementStyle(evt, element, field));

            static void ApplyElementStyle(GeometryChangedEvent evt, SketchElement<T> element, VisualElement field)
            {
                VisualElement container = (VisualElement)evt.target;
                string label = (string)container.userData;
                ConfigureLabeledElement(element, label, field);
            }
        }

        private static void ConfigureSketchElementProperty<T>(SketchElement<T> element, SerializedProperty property, VisualElement field, string nameOverride = null) where T : BindableElement
        {
            element.Container = new VisualElement();
            element.Container.userData = nameOverride;
            element.Container.RegisterCallbackOnce<GeometryChangedEvent>(evt => ApplyElementStyle(evt, property, element, field));

            static void ApplyElementStyle(GeometryChangedEvent evt, SerializedProperty prop, SketchElement<T> element, VisualElement field)
            {
                VisualElement container = (VisualElement)evt.target;
                string label = (string)container.userData != null ? container.userData.ToString() : prop.displayName;
                ConfigureLabeledElement(element, label, field);
            }
        }
        
        internal static SketchElement<IntegerField> SketchIntegerField(string name, int initial, bool isDelayed = false, ISketchManipulator<IntegerField> manipulator = null, EventCallback<ChangeEvent<int>> changeCallback = null)
        {
            SketchElement<IntegerField> element = new SketchElement<IntegerField>();
            IntegerField intField = new IntegerField();
            element.Field = intField;
            intField.isDelayed = isDelayed;
            if (manipulator != null)
            {
                manipulator.Initialize(intField);
                intField.AddManipulator(manipulator.GetBaseManipulator());
            }
            intField.value = initial;
            if(changeCallback != null)
                intField.RegisterValueChangedCallback(changeCallback);
            
            ConfigureSketchElementField(element, intField, name);
            return element;
        }

        internal static SketchElement<IntegerField> SketchIntegerProperty(SerializedProperty property, bool isDelayed = false, ISketchManipulator<IntegerField> manipulator = null, string nameOverride = null)
        {
            SketchElement<IntegerField> element = new SketchElement<IntegerField>();
            IntegerField intField = new IntegerField();
            element.Field = intField;
            intField.isDelayed = isDelayed;
            if (manipulator != null)
            {
                manipulator.Initialize(intField);
                intField.AddManipulator(manipulator.GetBaseManipulator());
            }
            intField.BindProperty(property);
            intField.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, intField, nameOverride);
            return element;
        }

        internal static SketchElement<FloatField> SketchFloatProperty(SerializedProperty property, bool isDelayed = false, ISketchManipulator<FloatField> manipulator = null, string nameOverride = null)
        {
            SketchElement<FloatField> element = new SketchElement<FloatField>();
            FloatField floatField = new FloatField();
            element.Field = floatField;
            floatField.isDelayed = isDelayed;
            if (manipulator != null)
            {
                manipulator.Initialize(floatField);
                floatField.AddManipulator(manipulator.GetBaseManipulator());
            }
            floatField.BindProperty(property);
            floatField.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, floatField, nameOverride);
            return element;
        }

        internal static SketchElement<Vector2Field> SketchVector2Property(SerializedProperty property, ISketchManipulator<Vector2Field> manipulator = null, string nameOverride = null)
        {
            SketchElement<Vector2Field> element = new SketchElement<Vector2Field>();
            Vector2Field vectorField = new Vector2Field();
            element.Field = vectorField;
            
            foreach (var floatField in vectorField.Query<FloatField>().ToList())
                floatField.style.marginRight = 0;

            bool isFirst = false;
            foreach (var label in vectorField.Query<Label>().ToList())
            {
                if (!isFirst)
                {
                    isFirst = true;
                    continue;
                }
                label.style.marginLeft = SketchRendererUIData.MinorIndentValue;
            }
            
            if (manipulator != null)
            {
                manipulator.Initialize(vectorField);
                vectorField.AddManipulator(manipulator.GetBaseManipulator());
            }
            vectorField.BindProperty(property);
            vectorField.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, vectorField, nameOverride);
            return element;
        }
        
        internal static SketchElement<Vector2IntField> SketchVector2IntProperty(SerializedProperty property, ISketchManipulator<Vector2IntField> manipulator = null, string nameOverride = null)
        {
            SketchElement<Vector2IntField> element = new SketchElement<Vector2IntField>();
            Vector2IntField vectorField = new Vector2IntField();
            element.Field = vectorField;
            
            foreach (var intField in vectorField.Query<IntegerField>().ToList())
                intField.style.marginRight = 0;

            bool isFirst = false;
            foreach (var label in vectorField.Query<Label>().ToList())
            {
                if (!isFirst)
                {
                    isFirst = true;
                    continue;
                }
                label.style.marginLeft = SketchRendererUIData.MinorIndentValue;
            }

            if (manipulator != null)
            {
                manipulator.Initialize(vectorField);
                vectorField.AddManipulator(manipulator.GetBaseManipulator());
            }
            vectorField.BindProperty(property);
            vectorField.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, vectorField, nameOverride);
            return element;
        }

        internal static SketchElement<Toggle> SketchBoolProperty(SerializedProperty property, ISketchManipulator<Toggle> manipulator = null, string nameOverride = null)
        {
            SketchElement<Toggle> element = new SketchElement<Toggle>();
            Toggle toggle = new Toggle();
            element.Field = toggle;
            if (manipulator != null)
            {
                manipulator.Initialize(toggle);
                toggle.AddManipulator(manipulator.GetBaseManipulator());
            }
            toggle.BindProperty(property);
            toggle.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, toggle, nameOverride);
            return element;
        }
        
        internal static SketchElement<EnumField> SketchEnumField(string name, Enum enumType, ISketchManipulator<EnumField> manipulator = null, EventCallback<ChangeEvent<Enum>> changeCallback = null)
        {
            SketchElement<EnumField> element = new SketchElement<EnumField>();
            EnumField enumField = new EnumField(enumType);
            element.Field = enumField;
            if (manipulator != null)
            {
                manipulator.Initialize(enumField);
                enumField.AddManipulator(manipulator.GetBaseManipulator());
            }
            enumField.value = enumType;
            if (changeCallback != null)
                enumField.RegisterValueChangedCallback(changeCallback);
            
            ConfigureSketchElementField(element, enumField, name);
            return element;
        }
        
        internal static SketchElement<EnumField> SketchEnumProperty(SerializedProperty property, Enum enumType, ISketchManipulator<EnumField> manipulator = null, string nameOverride = null)
        {
            SketchElement<EnumField> element = new SketchElement<EnumField>();
            EnumField enumField = new EnumField(enumType);
            element.Field = enumField;
            if (manipulator != null)
            {
                manipulator.Initialize(enumField);
                enumField.AddManipulator(manipulator.GetBaseManipulator());
            }
            enumField.BindProperty(property);
            enumField.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, enumField, nameOverride);
            return element;
        }
        
        internal static SketchElement<ColorField> SketchColorProperty(SerializedProperty property, ISketchManipulator<ColorField> manipulator = null, string nameOverride = null)
        {
            SketchElement<ColorField> element = new SketchElement<ColorField>();
            ColorField colorField = new ColorField();
            element.Field = colorField;
            if (manipulator != null)
            {
                manipulator.Initialize(colorField);
                colorField.AddManipulator(manipulator.GetBaseManipulator());
            }
            colorField.BindProperty(property);
            colorField.TrackPropertyValue(property);
            
            ConfigureSketchElementProperty(element, property, colorField, nameOverride);
            return element;
        }

        internal static SketchElement<TextField> SketchTextField(string name, bool isDelayed = false, ISketchManipulator<TextField> manipulator = null, EventCallback<ChangeEvent<string>> changeCallback = null)
        {
            SketchElement<TextField> element = new SketchElement<TextField>();
            TextField textField = new TextField();
            element.Field = textField;
            textField.isDelayed = isDelayed;
            if (manipulator != null)
            {
                manipulator.Initialize(textField);
                textField.AddManipulator(manipulator.GetBaseManipulator());
            }
            if (changeCallback != null)
                textField.RegisterValueChangedCallback(changeCallback);
            
            ConfigureSketchElementField(element, textField, name);
            return element;
        }

        internal static SketchElement<ObjectField> SketchObjectField(string name, Type objectType, Object initial, ISketchManipulator<ObjectField> manipulator = null, EventCallback<ChangeEvent<Object>> changeCallback = null)
        {
            SketchElement<ObjectField> element = new SketchElement<ObjectField>();
            ObjectField objectField = new ObjectField();
            element.Field = objectField;
            if (manipulator != null)
            {
                manipulator.Initialize(objectField);
                objectField.AddManipulator(manipulator.GetBaseManipulator());
            }
            objectField.objectType = objectType;
            if(changeCallback != null)
                objectField.RegisterValueChangedCallback(changeCallback);
            if(initial != null)
                objectField.SetValueWithoutNotify(initial);

            ConfigureSketchElementField(element, objectField, name);
            return element;
        }

        internal static SketchElement<Slider> SketchSlider(string name, float start, float end, 
            EventCallback<ChangeEvent<float>> changeCallback = null, SerializedProperty prop = null, Action<Slider, SerializedProperty> propCallback = null)
        {
            SketchElement<Slider> element = new SketchElement<Slider>();
            element.Container = new VisualElement();
            element.Container.userData = name;
            element.Container.RegisterCallbackOnce<GeometryChangedEvent>(evt => ApplySliderStyle(evt, element, start, end, changeCallback, prop, propCallback));

            static void ApplySliderStyle(GeometryChangedEvent evt, SketchElement<Slider> element, float start, float end,
                EventCallback<ChangeEvent<float>> changeCallback = null, SerializedProperty prop = null,
                Action<Slider, SerializedProperty> propCallback = null)
            {
                VisualElement container = (VisualElement)evt.target;
            
                Slider slider = new Slider(start, end);
                element.Field = slider;
                ConfigureLabeledElement(element, (string)container.userData, slider);
                
                if (changeCallback != null)
                    slider.RegisterValueChangedCallback(changeCallback);
                if (prop != null && propCallback != null)
                {
                    slider.TrackPropertyValue(prop, evt => propCallback(slider, evt));
                    propCallback(slider, prop);
                }
            }
            
            return element;
        }

        internal static SketchElement<Slider> SketchFloatSliderPropertyWithInput(SerializedProperty propertyBind, string nameOverride = null, EventCallback<ChangeEvent<float>> changeCallback = null)
        {
            SketchElement<Slider> element = new SketchElement<Slider>();
            element.Container = new VisualElement();
            element.Container.userData = nameOverride;
            element.Container.RegisterCallback<GeometryChangedEvent>(evt => ApplySliderWithInputStyle(evt, element, propertyBind, changeCallback));
            
            static void ApplySliderWithInputStyle(GeometryChangedEvent evt, SketchElement<Slider> element, SerializedProperty bind, EventCallback<ChangeEvent<float>> callback = null)
            {
                VisualElement container = (VisualElement)evt.target;
                container.style.flexDirection = FlexDirection.Row;
                container.style.justifyContent = Justify.SpaceBetween;
                
                var existingLabel = container.Q<Label>();
                if (existingLabel == null)
                {
                    string nameOverride = (string)container.userData;
                    element.Label = new Label(nameOverride != null ? nameOverride : bind.displayName);
                    element.Label.style.minWidth = SketchRendererUIData.MinLabelWidth;
                    element.Label.style.maxWidth = SketchRendererUIData.MaxLabelWidth;
                    element.Label.style.overflow = Overflow.Hidden;
                    element.Label.style.flexGrow = 1;
                    container.Add(element.Label);
                }

                VisualElement existingInputContainer = null;
                foreach (var child in container.Children())
                {
                    if (child is VisualElement && child.name == "inputContainer")
                    {
                        existingInputContainer = child;
                        break;
                    }
                }
                if (existingInputContainer == null)
                {
                    VisualElement inputContainer = new VisualElement();
                    inputContainer.name = "inputContainer";
                    inputContainer.style.flexDirection = FlexDirection.Row;
                    inputContainer.style.flexGrow = 1;
                    inputContainer.style.justifyContent = Justify.FlexEnd; 
                    SketchRendererUIUtils.AddWithMargins(container, inputContainer, CornerData.Empty);
                    existingInputContainer = inputContainer;
                }
                
                var existingInputField = existingInputContainer.Q<FloatField>();
                
                if (PropertyAttributeWrapper.CheckHasRangeAttribute(bind, out float min, out float max))
                {
                    bool hasSpaceForSlider = container.resolvedStyle.width - SketchRendererUIData.MaxLabelWidth >
                                             SketchRendererUIData.LabeledInputSliderMinWidth +
                                             SketchRendererUIData.LabeledSliderFixedInputWidth;
                    if (hasSpaceForSlider)
                    {
                        var existingFloatSlider = existingInputContainer.Q<Slider>();
                        if (existingFloatSlider == null)
                        {
                            //remove previous field so it gets readded at the end of the container
                            if (existingInputField != null)
                            {
                                existingInputField.Unbind();
                                existingInputContainer.Remove(existingInputField);
                            }

                            Slider sliderFloat = new Slider(min, max);
                            element.Field = sliderFloat;
                            sliderFloat.style.flexGrow = 1f;
                            var preexistingLabel = sliderFloat.Q<Label>(Slider.labelUssClassName);
                            if (preexistingLabel != null)
                                sliderFloat.Remove(preexistingLabel);
                            sliderFloat.BindProperty(bind);
                            sliderFloat.TrackPropertyValue(bind);
                            if (callback != null)
                                sliderFloat.RegisterValueChangedCallback(callback);

                            sliderFloat.style.minWidth = SketchRendererUIData.LabeledInputSliderMinWidth;
                            sliderFloat.style.maxWidth = SketchRendererUIData.LabeledInputSliderMaxWidth;
                            SketchRendererUIUtils.AddWithMargins(existingInputContainer, sliderFloat, CornerData.Empty);
                        }
                    }
                    else if(!hasSpaceForSlider)
                    {
                        var existingFloatSlider = existingInputContainer.Q<Slider>();
                        if (existingFloatSlider != null)
                        {
                            existingFloatSlider.Unbind();
                            existingInputContainer.Remove(existingFloatSlider);
                        }
                    }
                    
                    if (existingInputField == null)
                    {
                        FloatField inputField = new FloatField();
                        inputField.style.flexGrow = 0;
                        inputField.style.width = SketchRendererUIData.LabeledSliderFixedInputWidth;
                        var preexistingFloatLabel = inputField.Q<Label>(Slider.labelUssClassName);
                        if (preexistingFloatLabel != null)
                            inputField.Remove(preexistingFloatLabel);
                        inputField.BindProperty(bind);
                        inputField.TrackPropertyValue(bind);
                        if(callback != null)
                            inputField.RegisterValueChangedCallback(callback);
                       SketchRendererUIUtils.AddWithMargins(existingInputContainer, inputField, SketchRendererUIData.LabeledSliderFixedInputMargin);
                    }
                }
                else
                    throw new UnityException("[SketchRendererUI] This Field currently only supports properties with a RangeAttribute.");
            }
            
            return element;
        }
        
        internal static SketchElement<SliderInt> SketchIntSliderPropertyWithInput(SerializedProperty propertyBind, string nameOverride = null, EventCallback<ChangeEvent<int>> changeCallback = null)
        {
            SketchElement<SliderInt> element = new SketchElement<SliderInt>();
            element.Container = new VisualElement();
            element.Container.userData = nameOverride;
            element.Container.RegisterCallback<GeometryChangedEvent>(evt => ApplySliderWithInputStyle(evt, element, propertyBind, changeCallback));
            
            static void ApplySliderWithInputStyle(GeometryChangedEvent evt, SketchElement<SliderInt> element, SerializedProperty bind, EventCallback<ChangeEvent<int>> callback = null)
            {
                VisualElement container = (VisualElement)evt.target;
                container.style.flexDirection = FlexDirection.Row;
                container.style.justifyContent = Justify.SpaceBetween;
                
                var existingLabel = container.Q<Label>();
                if (existingLabel == null)
                {
                    string nameOverride = (string)container.userData;
                    element.Label = new Label(nameOverride != null ? nameOverride : bind.displayName);
                    element.Label.style.minWidth = SketchRendererUIData.MinLabelWidth;
                    element.Label.style.maxWidth = SketchRendererUIData.MaxLabelWidth;
                    element.Label.style.overflow = Overflow.Hidden;
                    element.Label.style.flexGrow = 1;
                    container.Add(element.Label);
                }

                VisualElement existingInputContainer = null;
                foreach (var child in container.Children())
                {
                    if (child is VisualElement && child.name == "inputContainer")
                    {
                        existingInputContainer = child;
                        break;
                    }
                }
                if (existingInputContainer == null)
                {
                    VisualElement inputContainer = new VisualElement();
                    inputContainer.name = "inputContainer";
                    inputContainer.style.flexDirection = FlexDirection.Row;
                    inputContainer.style.flexGrow = 1;
                    inputContainer.style.justifyContent = Justify.FlexEnd; 
                    SketchRendererUIUtils.AddWithMargins(container, inputContainer, CornerData.Empty);
                    existingInputContainer = inputContainer;
                }
                
                var existingInputField = existingInputContainer.Q<IntegerField>();
                
                if (PropertyAttributeWrapper.CheckHasRangeAttribute(bind, out float min, out float max))
                {
                    bool hasSpaceForSlider = container.resolvedStyle.width - SketchRendererUIData.MaxLabelWidth >
                                             SketchRendererUIData.LabeledInputSliderMinWidth +
                                             SketchRendererUIData.LabeledSliderFixedInputWidth;
                    if (hasSpaceForSlider)
                    {
                        var existingIntSlider = existingInputContainer.Q<SliderInt>();
                        if (existingIntSlider == null)
                        {
                            //remove previous field so it gets readded at the end of the container
                            if (existingInputField != null)
                            {
                                existingInputField.Unbind();
                                existingInputContainer.Remove(existingInputField);
                            }

                            SliderInt sliderInt = new SliderInt((int)min, (int)max);
                            element.Field = sliderInt;
                            sliderInt.style.flexGrow = 1f;
                            var preexistingLabel = sliderInt.Q<Label>(SliderInt.labelUssClassName);
                            if (preexistingLabel != null)
                                sliderInt.Remove(preexistingLabel);
                            sliderInt.BindProperty(bind);
                            sliderInt.TrackPropertyValue(bind);
                            if (callback != null)
                                sliderInt.RegisterValueChangedCallback(callback);

                            sliderInt.style.minWidth = SketchRendererUIData.LabeledInputSliderMinWidth;
                            sliderInt.style.maxWidth = SketchRendererUIData.LabeledInputSliderMaxWidth;
                            SketchRendererUIUtils.AddWithMargins(existingInputContainer, sliderInt, CornerData.Empty);
                        }
                    }
                    else if(!hasSpaceForSlider)
                    {
                        var existingIntSlider = existingInputContainer.Q<Slider>();
                        if (existingIntSlider != null)
                        {
                            existingIntSlider.Unbind();
                            existingInputContainer.Remove(existingIntSlider);
                        }
                    }
                    
                    if (existingInputField == null)
                    {
                        IntegerField inputField = new IntegerField();
                        inputField.style.flexGrow = 0;
                        inputField.style.width = SketchRendererUIData.LabeledSliderFixedInputWidth;
                        var preexistingIntLabel = inputField.Q<Label>(SliderInt.labelUssClassName);
                        if (preexistingIntLabel != null)
                            inputField.Remove(preexistingIntLabel);
                        inputField.BindProperty(bind);
                        inputField.TrackPropertyValue(bind);
                        if(callback != null)
                            inputField.RegisterValueChangedCallback(callback);
                       SketchRendererUIUtils.AddWithMargins(existingInputContainer, inputField, SketchRendererUIData.LabeledSliderFixedInputMargin);
                    }
                }
                else
                    throw new UnityException("[SketchRendererUI] This Field currently only supports properties with a RangeAttribute.");
            }
            
            return element;
        }
    }
}