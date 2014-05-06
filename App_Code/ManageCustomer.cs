using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

/// <summary>
/// Summary description for ManageCustomer
/// </summary>
public class ManageCustomer
{

    SqlConnection connect;
    Customer c;

	public ManageCustomer()
	{
        connect = new SqlConnection
            (ConfigurationManager.ConnectionStrings["AutomartConnectionString"].ConnectionString);
	}

    private SqlCommand WritePerson()
    {
        string sqlPerson = "Insert into Person (LastName, FirstName) Values (@LastName, @FirstName)";
        SqlCommand personCmd = new SqlCommand(sqlPerson, connect);
        personCmd.Parameters.AddWithValue("@LastName", c.LastName);
        personCmd.Parameters.AddWithValue("@FirstName", c.FirstName);

        return personCmd;
    }

    private SqlCommand WriteVehicle()
    {
        string sqlVehicle = "Insert into Customer.Vehicle(LicenseNumber, VehicleMake, VehicleYear, PersonKey) " +
           "Values(@License, @Make, @Year, ident_Current('Person'))";
        SqlCommand vehicleCmd = new SqlCommand(sqlVehicle, connect);
        vehicleCmd.Parameters.AddWithValue("@License", c.LicenseNumber);
        vehicleCmd.Parameters.AddWithValue("@Make", c.VehicleMake);
        vehicleCmd.Parameters.AddWithValue("@Year", c.VehicleYear);

        return vehicleCmd;
    }

    private SqlCommand WriteRegisteredCustomer()
    {
        string sqlRegisteredCustomer =
          "Insert into Customer.RegisteredCustomer(Email, CustomerPasscode, "
      + "CustomerPassword, CustomerHashedPassword, PersonKey) "
      + "Values(@Email, @Passcode, @password, @hashedpass, ident_Current('Person'))";

        PasscodeGenerator pg = new PasscodeGenerator();
        PasswordHash ph = new PasswordHash();
        int passcode = pg.GetPasscode();

        SqlCommand regCustomerCmd = new SqlCommand(sqlRegisteredCustomer, connect);
        regCustomerCmd.Parameters.AddWithValue("@Email", c.Email);
        regCustomerCmd.Parameters.AddWithValue("@Passcode", passcode);
        regCustomerCmd.Parameters.AddWithValue("@password", c.PlainPassword);
        regCustomerCmd.Parameters.AddWithValue("@hashedPass", ph.HashIt(c.PlainPassword, passcode.ToString()));

        return regCustomerCmd;
    }

    public void WriteCustomer(Customer c)
    {
        this.c=c;

        SqlTransaction tran = null;

        SqlCommand pCmd = WritePerson();
        SqlCommand vCmd = WriteVehicle();
        SqlCommand rCmd = WriteRegisteredCustomer();
       

        connect.Open();
        try
        {
            tran = connect.BeginTransaction();
            pCmd.Transaction = tran;
            vCmd.Transaction = tran;
            rCmd.Transaction = tran;
            pCmd.ExecuteNonQuery();
            vCmd.ExecuteNonQuery();
            rCmd.ExecuteNonQuery();
            tran.Commit();
        }
        catch (Exception ex)
        {
            tran.Rollback();
            throw ex;
        }
        finally
        {
            connect.Close();
        }
    }
}