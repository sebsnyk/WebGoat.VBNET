Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports log4net
Imports System.Reflection

Namespace OWASP.WebGoat.NET.App_Code.DB
    'NOT THREAD SAFE!
    Public Class DbProviderFactory
        Private Shared log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Public Shared Function Create(configFile As ConfigFile) As IDbProvider
            configFile.Load()

            Dim dbType As String = configFile.Get(DbConstants.KEY_DB_TYPE)

            log.Info("Creating provider for" & dbType)

            Select Case dbType
                Case DbConstants.DB_TYPE_MYSQL
                    Return New MySqlDbProvider(configFile)
                Case DbConstants.DB_TYPE_SQLITE
                    Return New SqliteDbProvider(configFile)
                Case Else
                    Throw New Exception(String.Format("Don't know Data Provider type {0}", dbType))
            End Select
        End Function
    End Class
End Namespace