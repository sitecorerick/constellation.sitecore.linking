<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="0191e669-3274-454b-8b87-85ce129728bc" namespace="Constellation.Sitecore" xmlSchemaNamespace="urn:Constellation.Sitecore" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSectionGroup name="Constellation">
      <configurationSectionProperties>
        <configurationSectionProperty>
          <containedConfigurationSection>
            <configurationSectionMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/SharedContentConfiguration" />
          </containedConfigurationSection>
        </configurationSectionProperty>
      </configurationSectionProperties>
    </configurationSectionGroup>
    <configurationSection name="SharedContentConfiguration" namespace="Constellation.Sitecore.Links" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="sharedContent">
      <elementProperties>
        <elementProperty name="SharedContentFolders" isRequired="false" isKey="false" isDefaultCollection="true" xmlName="sharedContentFolders" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/SharedContentFolders" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElementCollection name="SharedContentFolders" xmlItemName="sharedContentFolder" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/SharedContentFolder" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="SharedContentFolder">
      <attributeProperties>
        <attributeProperty name="TemplateName" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="templateName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="RootPath" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="rootPath" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ParentPageQuery" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="parentPageQuery" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="CategorizedBySiteFolder" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="categorizedBySiteFolder" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="PathToSiteFolder" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="pathToSiteFolder" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="FolderName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="folderName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/0191e669-3274-454b-8b87-85ce129728bc/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>