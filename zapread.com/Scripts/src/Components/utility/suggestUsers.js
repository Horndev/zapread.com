
import { postData } from '../../utility/postData';

export async function suggestUsers(searchTerm) {
  var matchedUsers = [];
  var data = await postData("/Comment/Mentions/", {
    searchstr: searchTerm.toString() // not sure if toString is needed here...
  });
  matchedUsers = data.users;
  return matchedUsers;
}