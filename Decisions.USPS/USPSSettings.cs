using System.ComponentModel;
using DecisionsFramework.Data.ORMapper;
using DecisionsFramework.Design.Properties;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.ServiceLayer.Actions;
using DecisionsFramework.ServiceLayer.Actions.Common;
using DecisionsFramework.ServiceLayer.Services.Accounts;
using DecisionsFramework.ServiceLayer.Services.Administration;
using DecisionsFramework.ServiceLayer.Services.Folder;
using DecisionsFramework.ServiceLayer.Utilities;
using System.Runtime.Serialization;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;

namespace Decisions.USPS
{
    public class USPSSettings : AbstractModuleSettings, IInitializable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public USPSSettings()
        {
            EntityName = "USPS Settings";
        }

        [ORMField]
        [WritableValue]
        private string userId;
        
        [DataMember]
        [RequiredProperty("The USPS Module Requires setting a User ID.")]
        [PropertyClassification(new string[] { "USPS Integration" }, "UserId", 1)]
        public string UserId
        {
            get => userId;
            set
            {
                userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

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

        public void Initialize()
        {
            // this will create it
            ModuleSettingsAccessor<USPSSettings>.GetSettings();
        }
        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
