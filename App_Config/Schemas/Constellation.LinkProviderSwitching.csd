<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="f6c18f70-4ce8-4811-aa7a-dd82768c6b58" namespace="Constellation.Sitecore" xmlSchemaNamespace="urn:Constellation.Sitecore" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="Type" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSectionGroup name="Constellation">
      <configurationSectionProperties>
        <configurationSectionProperty>
          <containedConfigurationSection>
            <configurationSectionMoniker name="/f6c18f70-4ce8-4811-aa7a-dd82768c6b58/SwitchingLinkProviderConfiguration" />
          </containedConfigurationSection>
        </configurationSectionProperty>
      </configurationSectionProperties>
    </configurationSectionGroup>
    <configurationSection name="SwitchingLinkProviderConfiguration" namespace="Constellation.Sitecore.Links" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="switchingLinkProvider">
      <attributeProperties>
        <attributeProperty name="DefaultLinkProviderType" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="defaultLinkProviderType" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f6c18f70-4ce8-4811-aa7a-dd82768c6b58/Type" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="LinkProviderRules" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="linkProviderRules" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/f6c18f70-4ce8-4811-aa7a-dd82768c6b58/LinkProviderRules" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElementCollection name="LinkProviderRules" xmlItemName="linkProviderRule" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/f6c18f70-4ce8-4811-aa7a-dd82768c6b58/LinkProviderRule" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="LinkProviderRule">
      <attributeProperties>
        <attributeProperty name="Site" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="site" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/f6c18f70-4ce8-4811-aa7a-dd82768c6b58/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="LinkProviderType" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="linkProviderType" isReadOnly="false" typeConverter="TypeNameConverter">
          <type>
            <externalTypeMoniker name="/f6c18f70-4ce8-4811-aa7a-dd82768c6b58/Type" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>