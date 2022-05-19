
import { postData } from '../../utility/postData';

export async function suggestTags(searchTerm) {
  var matched = [];
  var data = await postData("/api/v1/tag/mentions/list/", {
    SearchTerm: searchTerm
  });
  matched = data.Tags;
  return matched;
}