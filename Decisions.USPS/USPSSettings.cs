using System.ComponentModel;
using System.Runtime.Serialization;
using DecisionsFramework.Data.ORMapper;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Properties;
using DecisionsFramework.Design.Properties.Attributes;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.ServiceLayer.Actions;
using DecisionsFramework.ServiceLayer.Actions.Common;
using DecisionsFramework.ServiceLayer.Services.Accounts;
using DecisionsFramework.ServiceLayer.Services.Administration;
using DecisionsFramework.ServiceLayer.Services.Folder;
using DecisionsFramework.ServiceLayer.Utilities;

namespace Decisions.USPS;

[CategoryClassification(0, CATEGORY_AUTH)]
[CategoryClassification(1, CATEGORY_AUTH_DEPRECATED)]
public class USPSSettings : AbstractModuleSettings, IInitializable, INotifyPropertyChanged
{
    private const string CATEGORY_AUTH = "Authentication";
    private const string CATEGORY_AUTH_DEPRECATED = "Authentication (Deprecated)";

    public USPSSettings()
    {
        EntityName = "USPS Settings";
    }

    [ORMField, WritableValue] private string userId;
    [ORMField, WritableValue] private string oAuthTokenId;

    [PropertyClassification(1, "OAuth Token", CATEGORY_AUTH)]
    [TokenPicker]
    public string OAuthToken
    {
        get => oAuthTokenId;
        set
        {
            oAuthTokenId = value;
            OnPropertyChanged(nameof(OAuthToken));
        }
    }
    
    [DataMember]
    [RequiredProperty("The USPS Module Requires setting a User ID.")]
    [PropertyClassification(2, "UserId", CATEGORY_AUTH_DEPRECATED)]
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

        if (!canAdministrate) return [];
        
        return
        [
            new EditEntityAction(typeof(USPSSettings), "Edit", "Edits the Portal Settings object")
                {
                    MinEditorHeight = 400,
                    MinEditorWidth = 400,
                    IsDefaultGridAction = true
                }
        ];
    }

    public void Initialize()
    {
        ModuleSettingsAccessor<USPSSettings>.GetSettings();
    }
    
    #region OnPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
