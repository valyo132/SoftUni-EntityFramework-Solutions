using Microsoft.Data.SqlClient;
using System.Text;

SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;User Id=sa;Password=Endemole132!;TrustServerCertificate=true;");
connection.Open();

// Input for Problem 09
//int id = int.Parse(Console.ReadLine());
//string result = IncreaseMinionAgeWithCP(connection, id);
//Console.WriteLine(result);

// Input for Problem 08
//int[] numbers = Console.ReadLine().Split().Select(int.Parse).ToArray();
//string result = IncreaseMinionAge(connection, numbers);
//Console.WriteLine(result);

// Problem 07
//string result = PrintMinionsNames(connection);
//Console.WriteLine(result);

// Input for Problem 06
//int id = int.Parse(Console.ReadLine());
//string result = RemoveVillain(connection, id);
//Console.WriteLine(result);

// Input for Problem 05
//string countyName = Console.ReadLine();
//string result = ChangeTownNameCasing(connection, countyName);
//Console.WriteLine(result);

// Input for Problem 04
//string[] minionInfo = Console.ReadLine().Split(" ");
//string villainName = Console.ReadLine().Split(" ")[1];
//string result = AddNewMinion(connection, minionInfo, villainName);
//Console.WriteLine(result);

// Problem 03
//int id = int.Parse(Console.ReadLine());
//string result = MinionsNames(connection, id);
//Console.WriteLine(result);

// Problem 02
//GetVillanMinionsCount(connection);

connection.Close();

// Problem 09
static string IncreaseMinionAgeWithCP(SqlConnection connection, int id)
{
    StringBuilder sb = new StringBuilder();

    string executeCPQuery = "EXEC dbo.usp_GetOlder @id";
    SqlCommand executeCPCmd = new SqlCommand(executeCPQuery, connection);
    executeCPCmd.Parameters.AddWithValue("@id", id);

    executeCPCmd.ExecuteNonQuery();

    string getMinion = "SELECT Name, Age FROM Minions WHERE Id = @id";
    SqlCommand getMinionCmd = new SqlCommand(getMinion, connection);
    getMinionCmd.Parameters.AddWithValue("@id", id);

    SqlDataReader reader = getMinionCmd.ExecuteReader();

    using (reader)
    {
        while (reader.Read())
        {
            sb.AppendLine($"{reader["Name"]} is {reader["Age"]} years old.");
        }
    }

    return sb.ToString().TrimEnd();
}

// Problem 08
static string IncreaseMinionAge(SqlConnection connection, int[] minionsIds)
{
    StringBuilder sb = new StringBuilder();

    string increasMinionAgeQuery = "UPDATE Minions\r\n   SET Name = UPPER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1\r\n WHERE Id = @Id";

    foreach (var id in minionsIds)
    {
        SqlCommand increaseMinionAgeCmd = new SqlCommand(increasMinionAgeQuery, connection);
        increaseMinionAgeCmd.Parameters.AddWithValue("@Id", id);

        increaseMinionAgeCmd.ExecuteNonQuery();
    }

    string getAllMinionsQuery = "SELECT Name, Age FROM Minions";
    SqlCommand getAllMinionsWithAgeCmd = new SqlCommand(getAllMinionsQuery, connection);
    SqlDataReader reader = getAllMinionsWithAgeCmd.ExecuteReader();

    using (reader)
    {
        while (reader.Read())
        {
            sb.AppendLine(reader["Name"].ToString() + " " + reader["Age"].ToString());
        }
    }

    return sb.ToString().TrimEnd();
}

// Problem 07
static string PrintMinionsNames(SqlConnection connection)
{
    StringBuilder sb = new StringBuilder();

    string getAllMinionsQuery = "SELECT Name FROM Minions";
    SqlCommand command = new SqlCommand(getAllMinionsQuery, connection);
    SqlDataReader reader = command.ExecuteReader();

    using (reader)
    {
        var minions = new List<string>();

        while (reader.Read())
        {
            string minionName = reader["Name"].ToString();
            minions.Add(minionName);
        }

        while (minions.Any())
        {
            sb.AppendLine(minions.First());
            minions.RemoveAt(0);

            if (minions.Any())
            {
                sb.AppendLine(minions.Last());
                minions.RemoveAt(minions.Count - 1);
            }
        }
    }

    return sb.ToString().TrimEnd();
}

// Problem 06
static string RemoveVillain(SqlConnection connection, int villaidId)
{
    SqlTransaction transaction = connection.BeginTransaction();

    try
    {
        string getVillainQuery = "SELECT Name FROM Villains WHERE Id = @villainId";
        SqlCommand getVillainCmd = new SqlCommand(getVillainQuery, connection, transaction);
        getVillainCmd.Parameters.AddWithValue("@villainId", villaidId);

        object villainName = getVillainCmd.ExecuteScalar();

        if (villainName != null)
        {
            // Delete the relation between tables
            string deleteRelationQuery = "DELETE FROM MinionsVillains WHERE VillainId = @villainId";
            SqlCommand deleteRelationCmd = new SqlCommand(deleteRelationQuery, connection, transaction);
            deleteRelationCmd.Parameters.AddWithValue("@villainId", villaidId);

            int deletedRows = deleteRelationCmd.ExecuteNonQuery();

            // Delete the villain from the database
            string deleteVillainQuery = "SELECT Name FROM Villains WHERE Id = @villainId";
            SqlCommand deleteVillainCmd = new SqlCommand(deleteVillainQuery, connection, transaction);
            deleteVillainCmd.Parameters.AddWithValue("@villainId", villaidId);

            deleteRelationCmd.ExecuteNonQuery();

            transaction.Commit();

            return $"{villainName} was deleted." + Environment.NewLine +
                $"{deletedRows} minions were released.";
        }
        else
        {
            return "No such villain was found.";
        }
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        return ex.ToString();
    }
}

// Problem 05
static string ChangeTownNameCasing(SqlConnection connection, string countyName)
{
    StringBuilder sb = new StringBuilder();

    SqlTransaction transaction = connection.BeginTransaction();

    try
    {
        string getCountyName = "SELECT Name FROM Countries WHERE Name = @Name";
        SqlCommand getCountyNameCmd = new SqlCommand(getCountyName, connection, transaction);
        getCountyNameCmd.Parameters.AddWithValue("Name", countyName);

        object county = getCountyNameCmd.ExecuteScalar();

        if (county != null)
        {
            // Update towns names in Upper casing
            string updateTownNameCasingQuery = "UPDATE Towns\r\n   SET Name = UPPER(Name)\r\n WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name = @countryName)";

            SqlCommand updateTownNameCasingCmd = new SqlCommand(updateTownNameCasingQuery, connection, transaction);
            updateTownNameCasingCmd.Parameters.AddWithValue("@countryName", countyName);

            updateTownNameCasingCmd.ExecuteNonQuery();

            // Get the count of updated towns
            string getCountOfUpdatedTownsQuery = " SELECT COUNT(*)\r\n   FROM Towns as t\r\n   JOIN Countries AS c ON c.Id = t.CountryCode\r\n  WHERE c.Name = @countyName";
            SqlCommand getCountOfUpdatedTownsCmd = new SqlCommand(getCountOfUpdatedTownsQuery, connection, transaction);
            getCountOfUpdatedTownsCmd.Parameters.AddWithValue("@countyName", countyName);

            int townCount = (int)getCountOfUpdatedTownsCmd.ExecuteScalar();
            if (townCount == 0)
                return "No town names were affected.";

            sb.AppendLine($"{townCount} town names were affected.");

            // Get all updated towns
            string getAllTownsQuery = " SELECT t.Name \r\n   FROM Towns as t\r\n   JOIN Countries AS c ON c.Id = t.CountryCode\r\n  WHERE c.Name = @countryName";
            SqlCommand getAllTownsCmd = new SqlCommand(getAllTownsQuery, connection, transaction);
            getAllTownsCmd.Parameters.AddWithValue("@countryName", countyName);

            var towns = new List<string>();

            SqlDataReader reader = getAllTownsCmd.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    string town = reader["Name"].ToString();

                    towns.Add(town);
                }

                sb.AppendLine($"[{string.Join(", ", towns)}]");
            }

            transaction.Commit();
        }
        else
        {
            return "No town names were affected.";
        }
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        return ex.ToString();
    }

    return sb.ToString().TrimEnd();
}

// Problem 04
static string AddNewMinion(SqlConnection connection, string[] minionInfo, string villainName)
{
    string minionName = minionInfo[1];
    int minionAge = int.Parse(minionInfo[2]);
    string townName = minionInfo[3];

    StringBuilder sb = new StringBuilder();

    SqlTransaction transaction = connection.BeginTransaction();

    try
    {
        int townId = 0;

        string getTownId = "SELECT Id FROM Towns WHERE Name = @townName";
        SqlCommand getTownCommand = new SqlCommand(getTownId, connection, transaction);
        getTownCommand.Parameters.AddWithValue("@townName", townName);

        object town = getTownCommand.ExecuteScalar();
        if (town == null)
        {
            SqlCommand insertTownCommand = new SqlCommand("INSERT INTO Towns (Name) VALUES (@townName)", connection, transaction);
            insertTownCommand.Parameters.AddWithValue("@townName", townName);

            insertTownCommand.ExecuteNonQuery();
            sb.AppendLine($"Town {townName} was added to the database.");

            townId = (int)getTownCommand.ExecuteScalar();
        }
        else
        {
            townId = (int)town;
        }

        int villainId = 0;

        string getVillainQuery = "SELECT Id FROM Villains WHERE Name = @Name";
        SqlCommand getVillainNameCommand = new SqlCommand(getVillainQuery, connection, transaction);
        getVillainNameCommand.Parameters.AddWithValue("@Name", villainName);

        object villain = getVillainNameCommand.ExecuteScalar();
        if (villain == null)
        {
            string insertVillainQuery = "INSERT INTO Villains (Name, EvilnessFactorId)  VALUES (@villainName, 4)";
            SqlCommand insertVillainCommand = new SqlCommand(insertVillainQuery, connection, transaction);
            insertVillainCommand.Parameters.AddWithValue("@villainName", villainName);

            insertVillainCommand.ExecuteNonQuery();
            sb.AppendLine($"Villain {villainName} was added to the database.");

            villainId = (int)getVillainNameCommand.ExecuteScalar();
        }
        else
        {
            villainId = (int)villain;
        }

        string insertMinionQuery = "INSERT INTO Minions (Name, Age, TownId) VALUES (@name, @age, @townId)";
        SqlCommand insertMinionCommand = new SqlCommand(insertMinionQuery, connection, transaction);
        insertMinionCommand.Parameters.AddWithValue("@name", minionName);
        insertMinionCommand.Parameters.AddWithValue("@age", minionAge);
        insertMinionCommand.Parameters.AddWithValue("@townId", townId);

        insertMinionCommand.ExecuteNonQuery();

        string getMinionIdQuery = "SELECT Id FROM Minions WHERE Name = @Name AND Age = @Age";
        SqlCommand getMinionIdCommand = new SqlCommand(getMinionIdQuery, connection, transaction);
        getMinionIdCommand.Parameters.AddWithValue("@Name", minionName);
        getMinionIdCommand.Parameters.AddWithValue("@Age", minionAge);
        int minionId = (int)getMinionIdCommand.ExecuteScalar();

        string addMinionToVillianQuery = "INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (@minionId, @villainId)";
        SqlCommand addMinionToVillainCommand = new SqlCommand(addMinionToVillianQuery, connection, transaction);
        addMinionToVillainCommand.Parameters.AddWithValue("@minionId", minionId);
        addMinionToVillainCommand.Parameters.AddWithValue("@villainId", villainId);

        addMinionToVillainCommand.ExecuteNonQuery();

        sb.AppendLine($"Successfully added {minionName} to be minion of {villainName}.");

        transaction.Commit();
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        return ex.ToString();
    }

    return sb.ToString().TrimEnd();
}

// Problem 03
static string MinionsNames(SqlConnection connection, int id)
{
    StringBuilder sb = new StringBuilder();

    string villianNameQuey = $"SELECT Name FROM Villains WHERE Id = @Id";

    SqlCommand cmd = new SqlCommand(villianNameQuey, connection);
    cmd.Parameters.AddWithValue("id", id);

    string villainName = (string)cmd.ExecuteScalar();

    if (villainName == null)
    {
        return $"No villain with ID {id} exists in the database.";
    }

    sb.AppendLine($"Villain: {villainName}");

    string minionsQuery = "SELECT\r\n\t m.Name, \r\n\t m.Age\r\n  FROM MinionsVillains AS mv\r\n  JOIN Minions As m ON mv.MinionId = m.Id\r\n WHERE mv.VillainId = @Id\r\nORDER BY m.Name";

    SqlCommand getMinionsCommand = new SqlCommand(minionsQuery, connection);
    getMinionsCommand.Parameters.AddWithValue("id", id);

    using SqlDataReader reader = getMinionsCommand.ExecuteReader();
    if (!reader.HasRows)
    {
        sb.AppendLine("(no minions)");
    }
    else
    {
        int index = 1;

        while (reader.Read())
        {
            sb.AppendLine($"{index}. {reader["Name"]} {reader["Age"]}");
            index++;
        }
    }

    return sb.ToString().TrimEnd();
}

// Problem 02
static void GetVillanMinionsCount(SqlConnection connection)
{
    string query = "SELECT v.Name, COUNT(mv.VillainId) AS MinionsCount  \r\n    FROM Villains AS v \r\n    JOIN MinionsVillains AS mv ON v.Id = mv.VillainId \r\nGROUP BY v.Id, v.Name \r\n  HAVING COUNT(mv.VillainId) > 3 \r\nORDER BY COUNT(mv.VillainId)";

    SqlCommand command = new SqlCommand(query, connection);
    SqlDataReader reader = command.ExecuteReader();

    using (reader)
    {
        while (reader.Read())
        {
            string villanName = reader["Name"].ToString();
            int minionCoung = int.Parse(reader["MinionsCount"].ToString());

            Console.WriteLine($"{villanName} - {minionCoung}");
        }
    }
}

