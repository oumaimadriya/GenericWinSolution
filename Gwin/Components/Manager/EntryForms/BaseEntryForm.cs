﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using App.WinFrom.Validation;
using System.Reflection;
using App.Gwin.Attributes;
using System.Data.Entity;
using App.Gwin.Fields;
using App.Shared.AttributesManager;
using App.Gwin.Entities;
using System.Resources;
using App.Gwin.Shared.Resources;
using App.Gwin.Application.BAL;
using App.Gwin.Exceptions.Gwin;
using App.Gwin.Exceptions.Helpers;

namespace App.Gwin
{
    /// <summary>
    /// Formulaire Mère de Saisie, il permet de création des formulaire spécifique à chaque Entity
    /// </summary>
    public partial class BaseEntryForm : UserControl, IBaseEntryForm
    {
        #region Variables



        /// <summary>
        /// Indique si les champs seront automatiquement générer ou manuellement implémenter 
        /// par la classe Hérité
        /// </summary>
        [Obsolete]
        bool AutoGenerateField { set; get; }

        /// <summary>
        /// à supprimer , utilisez Service
        /// </summary>
        protected DbContext context { get; set; }
        /// <summary>
        /// Obtient ou définire l'entité représenté par cette formulaire
        /// </summary>
        public BaseEntity Entity { get; set; }

        /// <summary>
        /// l'instance de service de la gestion en cours
        /// 
        /// </summary> 
        public IGwinBaseBLO EntityBLO { get; set; }

        /// <summary>
        /// Message da validation des champs de la formulire
        /// </summary>
        protected MessageValidation MessageValidation;
        /// <summary>
        /// Indicate if the form is generated
        /// </summary>
        private bool isGeneratedForm { get; set; }

        /// <summary>
        /// Indique le controle qui contient la formulaire
        /// </summary>
        protected Control ConteneurFormulaire { get; set; }

        /// <summary>
        /// Critères de recherche selectioné dans le filtre
        /// </summary>
        protected Dictionary<string, object> CritereRechercheFiltre { set; get; }

        /// <summary>
        /// Indique si la saisie des valeurs provient de l'étape de l'initialisation 
        /// de la formulaire
        /// il est utiliser pour ne pas lancer les evénement de changement des champs 
        /// lors de la pase de l'initialisation
        /// </summary>
        public bool isStepInitializingValues { get; set; }

        /// <summary>
        /// Config Entity
        /// </summary>
        public ConfigEntity ConfigEntity { get; set; }
        #endregion

        #region Evénements
        public event EventHandler EnregistrerClick;
        public event EventHandler AnnulerClick;
        protected void onEnregistrerClick(Object sender, EventArgs e)
        {
            if (EnregistrerClick != null) EnregistrerClick(sender, e);
        }
        protected void onAnnulerClick(Object sender, EventArgs e)
        {
            if (AnnulerClick != null) AnnulerClick(sender, e);
        }
        #endregion

        #region Constructeurs
        /// <summary>
        /// Entry form
        /// </summary>
        /// <param name="EtityBLO"></param>
        public BaseEntryForm(
            IGwinBaseBLO EtityBLO,
            BaseEntity entity,
            Dictionary<string, object> critereRechercheFiltre,
            bool AutoGenerateField)
        {
            InitializeComponent();

            CheckPramIsNull.CheckParam_is_NotNull(EtityBLO, this, nameof(EtityBLO));

            // Params
            this.EntityBLO = EtityBLO;

            this.Entity = entity;
            this.CritereRechercheFiltre = critereRechercheFiltre;
            this.AutoGenerateField = AutoGenerateField;
            this.ConfigEntity = ConfigEntity.CreateConfigEntity(this.EntityBLO.TypeEntity);

            // Les valeus par défaux
            this.isStepInitializingValues = false;
            this.MessageValidation = new MessageValidation(errorProvider);


            // Préparation de l'objet Entity
            if (this.EntityBLO != null && this.Entity == null)
                this.Entity = (BaseEntity)EtityBLO.CreateEntityInstance();
            if ((this.Entity == null || this.Entity.Id == 0) && this.CritereRechercheFiltre != null)
                this.InitialisationEntityParCritereRechercheFiltre();

            // Conteneurs du formulaire
            this.ConteneurFormulaire = this.flowLayoutPanelForm;

            // Génération du Formulaire
            if (this.AutoGenerateField)
            {
                if (this.ConfigEntity == null)
                {
                    this.ConfigEntity = ConfigEntity.CreateConfigEntity(this.EntityBLO.TypeEntity);
                }

            }

        }
        public BaseEntryForm(IGwinBaseBLO service)
            : this(service, null, null, true) { }

        public BaseEntryForm()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                throw new GwinUsageModeException("This constructor is used just for DesineMode in Visal Studio, you can not use it per Code");
            InitializeComponent();
        }


        /// <summary>
        /// Initialisation de l'entity par les critère de recherche selection dans le filtre
        /// </summary>
        protected void InitialisationEntityParCritereRechercheFiltre()
        {

            // ? this.Entity.GetType();
            Type typeEntity = this.EntityBLO.TypeEntity;


            foreach (PropertyInfo item in ListeChampsFormulaire())
            {
                // Configuration de la propriété
                ConfigProperty attributesOfProperty = new ConfigProperty(item, this.ConfigEntity);

                Type typePropriete = item.PropertyType;
                string NomPropriete = item.Name;

                // Continue si une valeur de cette propriété existe dans le filtre
                if (!this.CritereRechercheFiltre.ContainsKey(item.Name))
                    continue;


                if (item.PropertyType.Name == "String")
                {
                    typeEntity
                         .GetProperty(item.Name)
                         .SetValue(this.Entity, this.CritereRechercheFiltre[item.Name].ToString());
                }
                if (typePropriete.Name == "Int32")
                {
                    typeEntity
                         .GetProperty(item.Name)
                         .SetValue(this.Entity, Convert.ToInt32(this.CritereRechercheFiltre[item.Name]));
                }

                if (typePropriete.Name == "DateTime")
                {
                    typeEntity
                        .GetProperty(item.Name)
                        .SetValue(this.Entity, Convert.ToDateTime(this.CritereRechercheFiltre[item.Name]));
                }


                if (attributesOfProperty.Relationship?.Relation == RelationshipAttribute.Relations.ManyToOne)
                {
                    BaseEntity valeur_filtre = this.EntityBLO
                        .CreateServiceBLOInstanceByTypeEntity(item.PropertyType)
                        .GetBaseEntityByID(Convert.ToInt64(this.CritereRechercheFiltre[item.Name]));
                    typeEntity.GetProperty(NomPropriete).SetValue(this.Entity, valeur_filtre);
                }
            }


        }
        #endregion

        /// <summary>
        /// Load and Create Field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BaseEntryForm_Load(object sender, EventArgs e)
        {
            // Génération du Formulaire


            this.CreateFieldIfNotGenerated();


        }



        #region validation
        [Obsolete]
        protected void ValiderTextBox(object sender, CancelEventArgs e, ErrorProvider errorProvider, String message = "")
        {

            TextBox controle = (TextBox)sender;
            if (message == "") message = "La saisie de ce champs est oblégatoir";
            if (controle.Text.Trim() == String.Empty)
            {
                errorProvider.SetError(controle, message);
                e.Cancel = true;
            }
            else
                errorProvider.SetError(controle, "");
        }

        /// <summary>
        /// Creation d'une instance de l'objet en cours
        /// </summary>
        /// <returns></returns>

        #endregion

        #region CreateInsce
        [Obsolete]
        public virtual BaseEntryForm CreateInstance(IGwinBaseBLO Service)
        {
            BaseEntryForm formilaire = (BaseEntryForm)Activator.CreateInstance(this.GetType(), Service);
            return formilaire;
        }
        /// <summary>
        /// Création d'une instance comme cette formulaire
        /// </summary>
        /// <returns></returns>
        public virtual BaseEntryForm CreateInstance(IGwinBaseBLO Service, BaseEntity entity, Dictionary<string, object> CritereRechercheFiltre)
        {
            BaseEntryForm formilaire = (BaseEntryForm)Activator.CreateInstance(this.GetType(), Service, entity, CritereRechercheFiltre, true);
            return formilaire;
        }
        /// <summary>
        /// Créer une instance de l'objet du formulaire
        /// </summary>
        /// <returns></returns>
        public virtual BaseEntity CreateObjetInstance()
        {
            return new BaseEntity();
        }
        #endregion


        /// <summary>
        /// Obient la liste des Propriété à utliser dans la formulaire
        /// </summary>
        /// <returns></returns>
        protected List<PropertyInfo> ListeChampsFormulaire()
        {
            // Obtien la liste des PropertyInfo par ordrer d'affichage
            var listeProprite = from i in this.EntityBLO.TypeEntity.GetProperties()
                                where i.GetCustomAttribute(typeof(EntryFormAttribute)) != null
                                orderby ((EntryFormAttribute)i.GetCustomAttribute(typeof(EntryFormAttribute))).Ordre
                                select i;
            return listeProprite.ToList<PropertyInfo>();
        }



        /// <summary>
        /// Orientation Horizontal de TabPage des du formulaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {

            TabControl ctlTab = (TabControl)sender;

            Graphics g = e.Graphics;
            String sText;
            int iX;
            int iY;

            SizeF sizeText;
            sText = ctlTab.TabPages[e.Index].Text;
            sizeText = g.MeasureString(sText, ctlTab.Font);
            iX = e.Bounds.Left + 6;
            iY = (int)(e.Bounds.Top + (e.Bounds.Height - sizeText.Height) / 2);
            g.DrawString(sText, ctlTab.Font, Brushes.Black, iX, iY);

        }


    }
}
