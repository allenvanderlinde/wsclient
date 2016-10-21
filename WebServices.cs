/*

The MIT License (MIT)

Copyright (c) 2016 Allen Vanderlinde

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BbWsClient;

namespace wsclient
{
    /**
     * @brief Primary web services class used to handle connection
     *          and initialization.
     */
    public class WebServices
    {
        /* Our web services wrappers accessible by other classes
            for pulling or manipulating data. */
        /** @brief Primary Learn web services wrapper. */
        private static WebserviceWrapper WS;
        /** @brief Context wrapper. */
        private static ContextWrapper ContextWS;
        /** @brief Utility wrapper. */
        private static UtilWrapper UtilWS;
        /** @brief User wrapper. */
        private static UserWrapper UserWS;

        /* Basic constants used for the purposes
            of the walk-through. */
        /** @brief Timeout length. */
        private const long lifetime     = 60000;
        /** @brief Some arbitrary tool name. */
        private const string tool       = "myWSClient";
        /** @brief Some arbitrary vendor name. */
        private const string vendor     = "BbAdmin";

        /* Some shareable variables. */
        /** @brief Host to connect to. */
        private static string host;

        /** @brief Session ID generated with successful connection. */
        private static string sessionId;
        /** @brief Learn object which holds info about proxy tool registration results. */
        private static RegisterToolResultVO toolRegResult;

        /** @brief List of entitlements to grant to proxy tool during registration. */
        private static string[] entitlements = {"Context.WS:emulateUser", "Context.WS:logout", "Context.WS:getMemberships",
            "Context.WS:getMyMemberships", "Util.WS:checkEntitlement", "User.WS:getServerVersion",
            "User.WS:initializeUserWS","User.WS:saveUser", "User.WS:getUser",
            "User.WS:deleteUser", "User.WS:saveObserverAssociation", "User.WS:getObservee",
            "User.WS:deleteAddressBookEntry", "User.WS:getAddressBookEntry", "User.WS:saveAddressBookEntry"};

        /** @brief Learn data sources object used to provide DSK of user record(s). */
        private static DataSourceVO[] dataSources;
        /** @brief Key/value pair object to look up data source keys for record(s). */
        private static Dictionary<string, string> dsk;

        /**
         * @brief Establishes connection to environment and initializes
         *          services.
         * @param[in] _host Host to connect to.
         * @retval bool True if connected successfully.
         */
        public static bool connectAndInitialize(string _host)
        {
            host = _host;
            try
            {
                // Instiantiate the primary web services wrapper
                WS = new WebserviceWrapper(host, vendor, tool, lifetime);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error occurred: {0}", e.Message);

                return false;
            }

            // Initialize the primary web services object
            WS.initialize_v1();

            if(WS.debugInfo().Equals("NO session"))
            {
                Console.WriteLine("Error: Unable to generate session.");

                return false;
            }

            sessionId = WS.debugInfo();
            Console.WriteLine("Session generated: {0}", sessionId);

            return true;
        }

        /**
         * @brief Initializes the necessary web services wrappers.
         * @retval bool True if successul.
         */
        public static bool initializeWrappers()
        {
            try
            {
                ContextWS = WS.getContextWrapper();

                /*
                 * This is a workaround for a bug
                 * that will let us expose all
                 * course and/or user information
                 * from an environment (including disabled
                 * records).
                 */
                ContextWS.emulateUser("administrator");

                UtilWS = WS.getUtilWrapper();
                UserWS = WS.getUserWrapper();
            }
            catch(System.Web.Services.Protocols.SoapException)
            {
                return false;
            }

            return true;
        }

        /**
         * @brief Registers a new tool to an environment.
         * @param[in] desc Abitrary tool description.
         * @param[in] initialSecret The required password defined in Manage Global Properties.
         * @param[in] sharedSecret The password the tool will be using to connect for future requests.
         * @retval bool True if registration successful.
         */
        public static bool register(string desc, string initialSecret, string sharedSecret)
        {
            toolRegResult = WS.registerTool(desc, initialSecret, sharedSecret, entitlements, null);

            if(toolRegResult.status)
            {
                return true;
            }
            else
                return false;
        }

        /**
         * @brief Log this tool into the host.
         * @param[in] sharedSecret The proxy tool's password.
         * @retval bool True if tool logged in successfully.
         */
        public static bool login(string sharedSecret)
        {
            if(!WS.loginTool(sharedSecret))
            {
                return false;
            }

            return true;
        }

        /**
         * @brief Logs out of the environment.
         */
        public static void logout()
        {
            Console.WriteLine("Logging out...");

            if(!WS.logout())
            {
                Console.WriteLine("\nThere were issues logging out:\n\t{0}", WebServices.getWS().getLastError());
            }
            else
            {
                Console.WriteLine("\nLogged out successfully.");
            }
        }

        /**
         * @brief This method pulls a list of data sources from the system
         *          and builds a key/value pair dictionary out of the
         *          DataSourceVO object array.
         * @retval bool True if successful.
         */
        public static bool lookUpDataSources()
        {
            try
            {
                dataSources = WebServices.getUtilWS().getDataSources();
                if (dataSources == null)
                    throw new NullReferenceException();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Error: Unable to pull list of data sources.\n\tMore info: {0}", WebServices.getWS().getLastError());

                return false;
            }

            dsk = new Dictionary<string, string>();

            for (int i = 0; i < dataSources.Length; i++)
            {
                dsk.Add(dataSources[i].id, dataSources[i].batchUid);
            }

            return true;
        }

        /**
         * @brief This method returns a data source's batch UID by
         *          specifying the corresponding pk1.
         * @param[in] pk1 The string representing the data source's pk1.
         * @retval string The batch UID of the data source.
         */
        public static string getDataSource(string pk1)
        {
            return dsk[pk1];
        }

        /* Get methods. */
        /**
         * @brief Returns a handle to the primary web services object.
         * @retval WebserviceWrapper The primary Learn web services wrapper.
         */
        public static WebserviceWrapper getWS()
        {
            return WS;
        }

        /**
         * @brief Returns a handle to the utility web services object
         *          used primarily to pull a list of data sources from
         *          the system.
         * @retval UtilWrapper The Learn utility web service wrapper.
         */
        public static UtilWrapper getUtilWS()
        {
            return UtilWS;
        }

        /**
         * @brief Returns a handle to the user web services object.
         * @retval UserWrapper The Learn user web service wrapper.
         */
        public static UserWrapper getUserWS()
        {
            return UserWS;
        }
    }
}