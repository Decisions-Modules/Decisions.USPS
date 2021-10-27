using DecisionsFramework;
using DecisionsFramework.Data.ORMapper;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Properties;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.ServiceLayer.Actions;
using DecisionsFramework.ServiceLayer.Actions.Common;
using DecisionsFramework.ServiceLayer.Services.Accounts;
using DecisionsFramework.ServiceLayer.Services.Administration;
using DecisionsFramework.ServiceLayer.Services.Folder;
using DecisionsFramework.ServiceLayer.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Decisions.USPS
{
    [Writable]
    [ORMEntity]
    [Exportable]
    [DataContract]
    public class USPSSettings : AbstractModuleSettings, INotifyPropertyChanged, IValidationSource
    {
        public USPSSettings()
        {
            this.EntityName = "USPS Settings";
        }

        [ORMField]
        [WritableValue]
        private string userId;

        [DataMember]
        [PropertyClassification(new string[] { "USPS Integration" }, "UserId", 1)]
        public string UserId
        {
            get { return userId; }
            set
            {
                userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        public static USPSSettings GetSettings()
        {
            return ModuleSettingsAccessor<USPSSettings>.GetSettings();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public ValidationIssue[] GetValidationIssues()
        {
            List<ValidationIssue> issues = new List<ValidationIssue>();
            if (string.IsNullOrWhiteSpace(UserId))
                issues.Add(new ValidationIssue(this, "User id is not defined", "Define user id", BreakLevel.Fatal, nameof(UserId)));

            return issues.ToArray();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public override BaseActionType[] GetActions(AbstractUserContext userContext, EntityActionType[] types)
        {
            Account userAccount = userContext.GetAccount();

            FolderPermission permission = FolderService.Instance.GetAccountEffectivePermission(
                new SystemUserContext(), this.EntityFolderID, userAccount.AccountID);

            bool canAdministrate = FolderPermission.CanAdministrate == (FolderPermission.CanAdministrate & permission) ||
                                    userAccount.GetUserRights<PortalAdministratorModuleRight>() != null ||
                                    userAccount.IsAdministrator();

            if (canAdministrate)
            {
                return new BaseActionType[]
                    {
                        new EditEntityAction(typeof(USPSSettings), "Edit", "Edits the Portal Settings object")
                        {
                            MinEditorHeight = 400,
                            MinEditorWidth = 400,
                            IsDefaultGridAction = true
                        }
                    };
            }
            else return new BaseActionType[0];
        }
    }
}
