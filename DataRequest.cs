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
    /** @defgroup Enumerations Enumerations
     * Data request enumerations.
     * @{
     */
    /**
     * @brief These are the various request
     * constants we can use to define our
     * filters with.
     * 
     * These correspond to the UserWSConstants
     * values found in your downloaded
     * Learn web services documentation:
     * 
     * User.WS/blackboard/ws/user/UserWSConstants.html
     */
    public enum RequestType
    {
        GET_USER_BY_NAME_WITH_AVAILABILITY      = 6     /** Request user information by GUI user ID. */
    };
    /** @} */

    /**
     * @brief Class which handles the actual request and stores the
     *          data we're looking for.
     */
    public class DataRequest
    {
        /** @brief Learn users object array to build from pulled data. */
        private UserVO[] users;
        /** @brief Filter to use as wrapper for specific request criteria. */
        private UserFilter uf;

        /**
         * @brief Here we construct a DataRequest object which sends
         *          the actual request, builds any learn objects, and
         *          stores the pulled data we're looking for.
         */
        public DataRequest(string userId, out bool success)
        {
            /* Pull available data sources
                to match to user record. */
            if(!WebServices.lookUpDataSources())
            {
                success = false;

                return;
            }
            
            /* Here we know we just want information on
             * an individual user by user ID (as opposed
             * to pk1), so we'll define the user filter
             * for the request and then build our VO
             * from it. */
            if(!lookUpUser(userId))
            {
                success = false;

                return;
            }

            success = true;
        }

        /**
         * @brief Here we look up the user's information based on
         *          the criteria in the user filter.
         * @param[in] userId The ID of the user we're interested in.
         * @retval bool True if no issues.
         */
        private bool lookUpUser(string userId)
        {
            uf = new UserFilter();
            uf.filterType = (int)RequestType.GET_USER_BY_NAME_WITH_AVAILABILITY;
            uf.filterTypeSpecified = true;

            /* Since we're only looking for 1 user's information,
             * we instindex 0 of the string[] for the filter. */
            try
            {
                uf.name = new string[] { userId };
            }
            catch(NullReferenceException)
            {
                return false;
            }

            /* Build the UserVO[] with information about
             * the requested user. */
            try
            {
                users = WebServices.getUserWS().getUser(uf);
                if(users == null)   // Most likely services aren't available for the tool
                    throw new NullReferenceException();
            }
            catch(NullReferenceException)
            {
                return false;
            }

            return true;
        }

        /**
         * @brief Get a UserVO object at the specified index. We'll
         *          only be using index 0 for this walk-through, however
         *          feel free to experiment.
         * @param[in] index The index of the UserVO object array.
         * @retval UserVO The user record at the specified index.
         */
        public UserVO getUserData(int index)
        {
            return users[index];
        }

        /**
         * @brief This method converts epoch time for a date into
         *          standard format ().
         * @param[in] epochTicks The number of ticks corresponding
         *                          to epoch time.
         * @retval string A string of the converted date in standard
         *                  format.
         */
        public static string toStandardDate(long? epochTicks)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return epoch.AddSeconds((long)epochTicks).ToShortDateString();
        }
    }
}
