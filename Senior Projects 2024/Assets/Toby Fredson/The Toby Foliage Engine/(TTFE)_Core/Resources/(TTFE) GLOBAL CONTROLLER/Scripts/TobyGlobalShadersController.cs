using System.Collections.Generic;
using UnityEngine;

namespace TobyFredson
{

    [ExecuteInEditMode]
    public class TobyGlobalShadersController : MonoBehaviour
    {

        #region Inspector Fields

        [SerializeField]
        private TobyWindType windType;

        [SerializeField]
        [Range(0f, 1f)]
        private float windStrength;

        [SerializeField]
        [Range(1f, 3f)]
        private float windSpeed;

        [Space]

        [SerializeField]
        [Range(-2f, 2f)]
        private float season;

        #endregion //Inspector Fields

        #region Private Fields

        protected TobyShaderValuesModel cachedAppliedValue;

        private readonly List<Material> matsGrassFoliage = new List<Material>();
        private readonly List<Material> matsTreeBark = new List<Material>();
        private readonly List<Material> matsTreeFoliage = new List<Material>();
        private readonly List<Material> matsTreeBillboard = new List<Material>();
		private readonly List<Material> matsGlobalController = new List<Material>();

        #endregion //Private Fields

        #region Unity Callbacks

        protected void Start() => RefreshList();

        protected void Update()
        {
            ApplyValues(GetNewValues());
        }

        #endregion //Unity Callbacks

        #region Public API  

        [ContextMenu("TobyFredson - Refresh")]
        public virtual void Refresh()
        {
            RefreshList();
            ApplyValues(GetNewValues(), true);
        }

        #endregion //Public API  

        #region Client Impl

        protected virtual void RefreshList()
        {
            matsGrassFoliage.Clear();
            matsTreeBark.Clear();
            matsTreeFoliage.Clear();
            matsTreeBillboard.Clear();
			matsGlobalController.Clear();

            var renderers = FindObjectsOfType<Renderer>();

            foreach (var ren in renderers)
            {
                var mats = ren.sharedMaterials;

                foreach (var mat in mats)
                {
                    RegisterMaterial(mat);
                }
            }
        }

        protected virtual void RegisterMaterial(Material mat)
        {
            if ((mat == null) || (mat.shader == null))
            {
                return;
            }

            if (mat.shader.name.Equals(TobyConstants.SHADER_NAME_GRASS_FOLIAGE))
            {
                matsGrassFoliage.Add(mat);
            }
            else if (mat.shader.name.Equals(TobyConstants.SHADER_NAME_TREE_BARK))
            {
                matsTreeBark.Add(mat);
            }
            else if (mat.shader.name.Equals(TobyConstants.SHADER_NAME_TREE_FOLIAGE))
            {
                matsTreeFoliage.Add(mat);
            }
            else if (mat.shader.name.Equals(TobyConstants.SHADER_NAME_TREE_BILLBOARD))
            {
                matsTreeBillboard.Add(mat);
            }
			else if (mat.shader.name.Equals(TobyConstants.SHADER_NAME_GLOBAL_CONTROLLER))
			{
				matsGlobalController.Add(mat);
			}
        }

        protected virtual void ApplyValues(
            TobyShaderValuesModel model, bool isForced = false)
        {
            if ((matsGrassFoliage.Count == 0) && (matsTreeBark.Count == 0) 
                && (matsTreeFoliage.Count == 0))
            {
                return;
            }

            if (!isForced && cachedAppliedValue.Equals(model))
            {
                return;
            }

            //Debug.Log($"{GetType().Name}.ApplyValues(): Test call isForced: {isForced}", gameObject);

            var mats = new List<Material>();
            mats.AddRange(matsTreeFoliage);
            mats.AddRange(matsTreeBark);
            mats.AddRange(matsGrassFoliage);
            mats.AddRange(matsTreeBillboard);
			mats.AddRange(matsGlobalController);
	
            foreach (var mat in mats)
            {
                ApplyValuesToMaterial(mat, model);
            }

            cachedAppliedValue = model;
        }

        private void ApplyValuesToMaterial(Material mat, TobyShaderValuesModel model)
        {
            SetShaderVariable(mat,
                        TobyConstants.SHADER_VAR_FLOAT_SEASON, model.season);
            SetShaderVariable(mat,
                TobyConstants.SHADER_VAR_FLOAT_WIND_STRENGTH, model.windStrength);
            SetShaderVariable(mat,
                TobyConstants.SHADER_VAR_FLOAT_WIND_SPEED, model.windSpeed);

            switch (model.windType)
            {
                case TobyWindType.GentleBreeze:
                    {
                        mat.EnableKeyword(TobyConstants.SHADER_WIND_TYPE_VALUE_GENTLEBREEZE);

                        mat.DisableKeyword(TobyConstants.SHADER_WIND_TYPE_VALUE_WINDOFF);
                        break;
                    }
                case TobyWindType.WindOff:
                    {
                        mat.EnableKeyword(TobyConstants.SHADER_WIND_TYPE_VALUE_WINDOFF);

                        mat.DisableKeyword(TobyConstants.SHADER_WIND_TYPE_VALUE_GENTLEBREEZE);
                        break;
                    }
            }
        }

        protected void SetShaderVariable(Material mat, string shaderVar, float value)
            => mat.SetFloat(shaderVar, value);

        protected void SetShaderToggle(Material mat, string shaderToggle, bool value)
        {
            if (value)
            {
                mat.EnableKeyword(shaderToggle);
            }
            else
            {
                mat.DisableKeyword(shaderToggle);
            }
        }

        protected TobyShaderValuesModel GetNewValues()
        {
            var newModel = new TobyShaderValuesModel();

            newModel.season = season;

            newModel.windType = windType;
            newModel.windStrength = windStrength;
            newModel.windSpeed = windSpeed;

            return newModel;
        }

        #endregion //Client Impl

    }

}