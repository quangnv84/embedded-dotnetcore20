﻿using System;
using System.Collections.Generic;
using System.Text;
using SQLDatabase.Net.SQLDatabaseClient;
// use ORMClient namespace can be changed to any other namespace
using SQLDatabase.Net.ORMClient;

namespace SQLDatabase.Net.Core.Examples
{
    // Start of Database objects
    // C# classes which will be used as database objects.
    // Class properties are decorated with DBColumn attribute
       
    class Departments
    {
        [DBColumn(AutoIncrement = true, PrimaryKey = true)]
        public int DepartmentId { get; set; } = 0; // Initialized with 0 since departmentid is auto generated by sql engine and does not require user to assign values.
        [DBColumn(NotNull = true)]
        public string DepartmentName { get; set; }
    }

    class Employees
    {
        [DBColumn(AutoIncrement = true, PrimaryKey = true)]
        public int EmployeeId { get; set; }

        [DBColumn]
        public string FirstName { get; set; }

        [DBColumn(NotNull = true)]
        public string LastName { get; set; } // Last name must be present and cannot be null.        

        [DBColumn(ForeignKey = true, ForeignKeyTable = "Departments" , ForeignKeyColumn = "DepartmentId")]
        public int DepartmentId { get; set; } //Create column with foreign key which refers to Departments table DepartmentId column.
    }

    class DepartmentEmployees
    {
        [DBColumn]
        public string FirstName { get; set; }

        [DBColumn]
        public string LastName { get; set; }

        [DBColumn]
        public string DepartmentName { get; set; }
    }

    class vw_Top10CustomersByOrders
    {
        [DBColumn]
        public int CustomerId { get; set; }
        [DBColumn]
        public string FirstName { get; set; }
        [DBColumn]
        public string LastName { get; set; }
        [DBColumn]
        public int TotalOrders { get; set; }
        [DBColumn]
        public string OrdersTotal { get; set; }
    }
    // End of Database Objects


    class ORMClientExamples
    {
        static string InMemoryConnectionString = "SchemaName=db;uri=@memory;";
        static SqlDatabaseConnection InMemoryConnection = new SqlDatabaseConnection(InMemoryConnectionString);

        public static void StartExamples()
        {
            if (InMemoryConnection.State != System.Data.ConnectionState.Open)
                InMemoryConnection.Open();

            LoadDepartments();
            LoadEmployees();
            EmployeesWithDepartments();
            DatabaseViews();

            if (InMemoryConnection.State != System.Data.ConnectionState.Open)
                InMemoryConnection.Close();

            InMemoryConnection.Dispose();

            Console.WriteLine();
            

        }

        static void LoadDepartments()
        {
            Console.WriteLine("Loading Departments example..");
            Console.WriteLine();

            //Initalize ORM client with Departments object
            SqlDatabaseOrmClient<Departments> depts = new SqlDatabaseOrmClient<Departments>(InMemoryConnection);
            depts.CreateTable();//Create table departments if it does not exists.

            // Add department one by one
            // We do not need to provide departmentid as it is autogenerated.
            Departments dept = new Departments(); //Create new instance of Departments class
            dept.DepartmentName = "Administration";
            depts.Add(dept);

            dept = new Departments
            {
                DepartmentName = "Sales & Marketing"
            };
            depts.Add(dept);

            //Following will produce error due to DepartmentName may not be null.
            try
            {
                dept = new Departments
                {
                    DepartmentName = null
                };
                depts.Add(dept);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error due to null: {0}", e.Message);
                Console.WriteLine();
            }
            // End of Add department one by one

            // Adding multiple departments using list
            List<Departments> departmentslist = new List<Departments>();
            departmentslist.Add(new Departments() { DepartmentName = "HR" });
            departmentslist.Add(new Departments() { DepartmentName = "Information Technology" });
            depts.AddRange(departmentslist);

            // Get all records.
            Console.WriteLine("Example to get all records for departments..");
            foreach (Departments d in depts.GetAll())
            {
                Console.Write(string.Format("Id: {0} \t Name: {1}", d.DepartmentId, d.DepartmentName));
                Console.WriteLine();
            }
            Console.WriteLine(); //Empty line

            // Optional for testing, get records using sql 
            Console.WriteLine("Example to fetch records using SQL statement....");
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(InMemoryConnection))
            {
                cmd.CommandText = "SELECT * FROM Departments; ";
                SqlDatabaseDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Console.Write(string.Format("Id: {0} \t Name: {1}", dr["DepartmentId"], dr["DepartmentName"]));
                    Console.WriteLine();
                }
            }
            Console.WriteLine(); //Empty line


            //Updating existing records
            Departments DeptToUpdate = new Departments
            {
                DepartmentId = 4,
                DepartmentName = "IT"
            };
            depts.Update(DeptToUpdate);

            //Filtering records
            SqlDatabaseOrmClient<Departments>.Filter<Departments> DeptFilter = new SqlDatabaseOrmClient<Departments>.Filter<Departments>();
            DeptFilter.WhereWithOR(item => item.DepartmentName == "HR");
            DeptFilter.WhereWithOR(item => item.DepartmentId == 4);

            DeptFilter.LimitAndOffSet(2, 0);
            DeptFilter.OrderByDescending(item => item.DepartmentId);
           

            // Get records based on filter.
            Console.WriteLine("Filtered Records example....");
            foreach (Departments d in depts.Find(DeptFilter))
            {
                Console.Write(string.Format("Id: {0} \t Name: {1}", d.DepartmentId, d.DepartmentName));
                Console.WriteLine();
            }

            //Drop table
            //depts.DropTable(new Departments());
        }

        static void LoadEmployees()
        {
            Console.WriteLine("\nLoading Employees class example..");
            Console.WriteLine();

            // Assign the connection and initialize the ORM client.
            SqlDatabaseOrmClient<Employees> emps = new SqlDatabaseOrmClient<Employees>(InMemoryConnection);
            emps.CreateTable();//Create table EmployeeMaster if it does not exists.

            // Add EmployeeMaster one by one
            // We do not need to provide EmployeeId as it is autogenerated.
            Employees emp = new Employees(); //Create new instance of Departments class
            emp.FirstName = "John";
            emp.LastName = "Dekota";
            emp.DepartmentId = 2;
            emps.Add(emp);

            emp = new Employees
            {
                FirstName = "Mary",
                LastName = "Denware",
                DepartmentId = 3
            };
            emps.Add(emp);

            emp = new Employees
            {
                FirstName = "Scott",
                LastName = "Hamilton",
                DepartmentId = 2
            };
            emps.Add(emp);
            // End of Add department one by one

            // Adding multiple Employees using list
            List<Employees> Employeeslist = new List<Employees>();
            Employeeslist.Add(new Employees() { FirstName = "Guru" , LastName = "Manna", DepartmentId = 3 });
            Employeeslist.Add(new Employees() { FirstName = "Robert", LastName = "Olipo", DepartmentId = 1 });
            Employeeslist.Add(new Employees() { FirstName = "John", LastName = "Doe", DepartmentId = 3 });
            emps.AddRange(Employeeslist);

            // Get all records.
            Console.WriteLine("Example to get all records for employees..");
            foreach (Employees e in emps.GetAll())
            {
                Console.Write(string.Format("Id: {0} \t FirstName: {1} \t LastName: {1} \t DepartmentId: {1}", e.EmployeeId, e.FirstName, e.LastName, e.DepartmentId));
                Console.WriteLine();
            }
            Console.WriteLine(); //Empty line  
        }


        static void EmployeesWithDepartments()
        {
            Console.WriteLine("\nExample of loading employees and departments, it acts like SQL view..");
            Console.WriteLine();

            //Initialize the ORM client
            SqlDatabaseOrmClient<DepartmentEmployees> view = new SqlDatabaseOrmClient<DepartmentEmployees>(InMemoryConnection);

            IList<DepartmentEmployees> ListOfDepartmentEmployees = view.GetByNaturalJoin(new Employees(), new Departments());

            foreach (DepartmentEmployees e in ListOfDepartmentEmployees)
            {
                Console.Write(string.Format("FirstName: {0} \t LastName: {1} \t Department: {2}", e.FirstName, e.LastName, e.DepartmentName));
                Console.WriteLine();
            }
            Console.WriteLine(); //Empty line  

        }

        static void DatabaseViews()
        {
            Console.WriteLine("\nLoading data from SQL based database views....");
            Console.WriteLine();

            string ExampleDatabaseFile = System.IO.Path.Combine(
                System.IO.Directory.GetParent(
                    System.IO.Path.GetDirectoryName(
                        AppDomain.CurrentDomain.BaseDirectory)).Parent.Parent.FullName, "Orders.db"
                        );

            if (!System.IO.File.Exists(ExampleDatabaseFile))
            {
                Console.WriteLine("Unable to find Orders.db exiting..");
                return;
            }

            string constr = "uri=file://" + ExampleDatabaseFile;
            using (SqlDatabaseConnection cnn = new SqlDatabaseConnection(constr))
            {                
                cnn.Open();

                //Initialize the ORM client
                SqlDatabaseOrmClient<vw_Top10CustomersByOrders> Top10CustomersByOrders = new SqlDatabaseOrmClient<vw_Top10CustomersByOrders>(cnn);

                foreach (vw_Top10CustomersByOrders o in Top10CustomersByOrders.GetAll())
                {
                    Console.Write(string.Format("FirstName: {0} \t LastName: {1} \t OrderCount: {2} \t OrderTotal: {3}", o.FirstName, o.LastName, o.TotalOrders, o.OrdersTotal));
                    Console.WriteLine();
                }
                Console.WriteLine(); //Empty line  

                Console.WriteLine("Since views are readonly following will produce an error when inserting, updating or deleting from view.");
                try
                {
                    vw_Top10CustomersByOrders v10 = new vw_Top10CustomersByOrders();
                    v10.CustomerId = 10001;
                    v10.FirstName = "John";
                    v10.LastName = "Doe";
                    v10.TotalOrders = 100;
                    v10.OrdersTotal = "$1";
                    Top10CustomersByOrders.Add(v10);
                } catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine(); //Empty line  
            }
           

        }
    }
}
