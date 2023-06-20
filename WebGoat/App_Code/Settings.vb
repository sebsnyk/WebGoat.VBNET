Imports System
Imports OWASP.WebGoat.NET.App_Code.DB
Imports System.IO
Imports System.Web
Imports log4net
Imports System.Reflection
Imports System.Diagnostics
Imports log4net.Config
Imports log4net.Appender
Imports log4net.Layout

Namespace OWASP.WebGoat.NET.App_Code
    Public Class Settings
        Public Shared ReadOnly DefaultConfigName As String = String.Format("Default.{0}", DbConstants.CONFIG_EXT)
        Private Const PARENT_CONFIG_PATH As String = "Configuration"
        Private Const DATA_FOLDER As String = "App_Data"
        Private Const DEFAULT_SQLITE_NAME As String = "webgoat_coins.sqlite"

        Private Shared _lock As New Object()
        Private Shared _inited As Boolean = False

        Private Shared log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Public Shared Sub Init(server As HttpServerUtility)
            SyncLock _lock
                If Debugger.IsAttached Then
                    BasicConfigurator.Configure()
                Else
                    XmlConfigurator.Configure()
                End If

                Dim configPath As String = Path.Combine(PARENT_CONFIG_PATH, DefaultConfigName)
                DefaultConfigPath = server.MapPath(configPath)

                RootDir = server.MapPath(".")

                log.Debug("DYLD_FALLBACK_LIBRARY_PATH: " & Environment.GetEnvironmentVariable("DYLD_FALLBACK_LIBRARY_PATH"))
                log.Debug("PWD: " & Environment.CurrentDirectory)

                'By default if there's no config let's create a sqlite db.
                Dim defaultConfigPath As String = DefaultConfigPath

                Dim sqlitePath As String = Path.Combine(DATA_FOLDER, DEFAULT_SQLITE_NAME)
                sqlitePath = server.MapPath(sqlitePath)

                If Not File.Exists(defaultConfigPath) Then
                    Dim file As New ConfigFile(defaultConfigPath)

                    file.Set(DbConstants.KEY_DB_TYPE, DbConstants.DB_TYPE_SQLITE)
                    file.Set(DbConstants.KEY_FILE_NAME, sqlitePath)
                    file.Save()

                    CurrentConfigFile = file
                Else
                    CurrentConfigFile = New ConfigFile(defaultConfigPath)
                    CurrentConfigFile.Load()
                End If

                CurrentDbProvider = DbProviderFactory.Create(CurrentConfigFile)
                _inited = True
            End SyncLock
        End Sub

        Public Shared Property RootDir As String

        Public Shared Property CurrentDbProvider As IDbProvider

        Public Shared Property DefaultConfigPath As String

        Public Shared Property CurrentConfigFile As ConfigFile

        Public Shared ReadOnly Property Inited As Boolean
            Get
                SyncLock _lock
                    Return _inited
                End SyncLock
            End Get
        End Property
    End Class
End Namespace