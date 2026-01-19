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

public class USPSSettings : AbstractModuleSettings, IInitializable, INotifyPropertyChanged
{
    private const string CATEGORY_AUTH = "Authentication";
    private const string CATEGORY_AUTH_DEPRECATED = "Authentication (Deprecated)";

    public USPSSettings()
    {
        EntityName = "USPS Settings";
    }

    [ORMField, WritableValue] private string userId;

    
    [DataMember]
    [RequiredProperty("The USPS Module Requires setting a User ID.")]
    [PropertyClassification(1, "UserId", CATEGORY_AUTH_DEPRECATED)]
    public string UserId
    {
        get => userId;
        set
        {
            userId = value;
            OnPropertyChanged(nameof(UserId));
        }
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
