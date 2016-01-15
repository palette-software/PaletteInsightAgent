﻿namespace DataTableWriter.Connection
{
    /// <summary>
    /// Describes the information required to connect to a remote database.
    /// </summary>
    public interface IDbConnectionInfo
    {
        string DatabaseName { get; set; }
        string Password { get; set; }
        // gyulalaszlo: the nullability of the port has been removed
        int? Port { get; set; }
        string Server { get; set; }
        string Username { get; set; }

        bool Valid();
    }
}