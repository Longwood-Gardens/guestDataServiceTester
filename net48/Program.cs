using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using SecureRemotePassword;

using guestDataServiceTester_net48.Objects;
using System.Text.Json;
using System.Net.Http.Headers;

namespace guestDataServiceTester_net48
{
    internal class Program
    {
        // Base URL of the API...
        const string BASE_URI = "https://rest-dev.longwoodgardens.org/guestDataService";

        // Your API ID...
        const string ID = "";

        // Your API Secret Key...
        static readonly byte[] SECRET_BYTES = "".HexToBytes();

        static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static string Decrypt(string cipherText, byte[] key)
        {
            var cipherTextParts = cipherText.Split('.');
            if (2 != cipherTextParts.Length) throw new ArgumentException("" +
                "Should be of the form [base64encoded iv].[base64encoded ciphertext]",
                nameof(cipherText));

            using (Aes aes = Aes.Create())
            {
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = Convert.FromBase64String(cipherTextParts[0]);
                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {

                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        byte[] b = Convert.FromBase64String(cipherTextParts[1]);
                        cs.Write(b, 0, b.Length);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }

            }
        }

        private static async Task<string> Authenticate(string clientId, byte[] secret)
        {
            var client = new SrpClient();
            var clientEphemeral = client.GenerateEphemeral();

            var authHttpClient = new HttpClient();
            var challengeResponseHTTP = await authHttpClient.PostAsync(
                $"{BASE_URI}/Auth/Challenge",
                new StringContent(JsonSerializer.Serialize(
                    new AuthChallengeRequest
                    {
                        Id = clientId,
                        EphemeralPublic = clientEphemeral.Public
                    }), Encoding.UTF8, "application/json"));

            var challengeResponse = JsonSerializer.Deserialize<AuthChallengeResponse>(
                challengeResponseHTTP.Content.ReadAsStringAsync().GetAwaiter().GetResult(),
                JSON_OPTIONS);

            if (!challengeResponseHTTP.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Challenge Failed: " +
                    $"HTTP-{challengeResponseHTTP.StatusCode}: " +
                    $"{challengeResponse?.Message}");
                throw new System.Security.Authentication.AuthenticationException("SRP Challenge Failed");
            }

            // Derive our private key from the Salt retrieved the from server (via the Challenge Request)
            // + the ClientID + the secret
            var privateKey = client.DerivePrivateKey(challengeResponse.Salt,
                clientId, Encoding.UTF8.GetString(secret));

            // Use our ClientEphemeral Keypair's Secret key, the Server's Epmemeral Public key
            // and the salt, ID, and private key to derive an SrpSession (Client Session)...
            var clientSession = client.DeriveSession(
                clientEphemeral.Secret,
                challengeResponse.EphemeralPublic,
                challengeResponse.Salt,
                ID,
                privateKey);


            var authenticateResponseHTTP = await authHttpClient.PostAsync(
                $"{BASE_URI}/Auth/Authenticate",
                new StringContent(JsonSerializer.Serialize(
                    new AuthenticateRequest
                    {
                        ChallengeId = challengeResponse.ChallengeID,
                        Proof = clientSession.Proof
                    }), Encoding.UTF8, "application/json"));

            var authenticateResponse = JsonSerializer.Deserialize<AuthenticateResponse>(
                await authenticateResponseHTTP.Content.ReadAsStringAsync(),JSON_OPTIONS);

            if (!authenticateResponseHTTP.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Authenticate Failed: " +
                    $"HTTP-{authenticateResponseHTTP.StatusCode}: " +
                    $"{authenticateResponse?.Message}");
                throw new System.Security.Authentication.AuthenticationException(
                    "SRP Authenticate Failed");
            }

            // Verify the Server's "Proof" response before accepting this session...
            client.VerifySession(clientEphemeral.Public, clientSession, authenticateResponse.Proof);

            // The EBearer field of the authenticateResponse is the bearer token, but it has been
            // AES-256-CFB encrypted using the SRP Session Key found in clientSession.key...
            // Hence, we decrypt the EBearer field to get our actual Bearer token.
            return Decrypt(authenticateResponse.EBearer, clientSession.Key.HexToBytes());
        }

        static void Main(string[] args)
        {
            var httpClient = new HttpClient();
            var bearerToken = Authenticate(ID, SECRET_BYTES).GetAwaiter().GetResult();
            Console.WriteLine($"Bearer Token={bearerToken}");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var MemberID = "167120000";
            Console.Write($"\n\nMemberID={MemberID}, (Press any key to make request, Q=Quit, C=Change Member ID)...");
            var key = Console.ReadKey(true).Key;
            Console.WriteLine();

            while (ConsoleKey.Q != key)
            {
                var getDiscountHTTP = httpClient.GetAsync(
                    $"{BASE_URI}/GuestData/membershipFoodBeverageDiscount?MembershipID={MemberID}").GetAwaiter().GetResult();

                // Display the HTTP Headers....
                Console.WriteLine("====================================");
                foreach (var hdr in getDiscountHTTP.Headers)
                {
                    Console.WriteLine($"Header: {hdr.Key}: {string.Join("\n", hdr.Value)}");
                }
                Console.WriteLine("====================================\n");

                if (getDiscountHTTP.IsSuccessStatusCode
                        || System.Net.HttpStatusCode.NotFound == getDiscountHTTP.StatusCode)
                {
                    Console.WriteLine("Response: ");
                    Console.WriteLine(getDiscountHTTP.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    //var discountResponse = await getDiscountHTTP.Content
                    //    .ReadFromJsonAsync<GuestDataResponse<FoodBeverageDiscountResponse>>();
                    //Console.WriteLine(JsonSerializer.Serialize(discountResponse.Data));
                }
                else if (System.Net.HttpStatusCode.Unauthorized == getDiscountHTTP.StatusCode)
                {
                    Console.WriteLine("Re-Authenticating...");
                    bearerToken = Authenticate(ID, SECRET_BYTES).GetAwaiter().GetResult();
                    Console.WriteLine($"Bearer Token: {bearerToken}");
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer", bearerToken);
                }
                else Console.WriteLine(getDiscountHTTP.StatusCode);

                Console.Write($"\n\nMemberID={MemberID}, (Press any key to make request, Q=Quit, C=Change Member ID)...");
                key = Console.ReadKey(true).Key;
                Console.WriteLine();
                if (ConsoleKey.C == key)
                {
                    Console.Write("New Member ID (enter): ");
                    MemberID = Console.ReadLine();
                }
            }
            Console.WriteLine("Done.");
        }
    }
}
