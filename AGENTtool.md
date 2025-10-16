# Tool Validation Checklist

Below is a compact, **clean** table that lists every tool available in
the repository, its category, whether it has already been tested in the
current environment (`[x]`), and a minimal example payload that can be
copied into the CLI.

> **Note** – Replace placeholder values (`myorg`, `myrepo`, `abcd1234`,etc.) with your real data before running.

| Tool                                                 | Category        | Tested | Example                                                                                                                                          |
| ---------------------------------------------------- | --------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| `shell`                                              | Shell           | `[x]`  | `echo "Hello, world!"`                                                                                                                           |
| `update_plan`                                        | Planner         | `[x]`  | `{"explanation":"test","plan":[{"step":"dummy","status":"pending"}]}`                                                                            |
| `docker_gateway__fetch`                              | HTTP            | `[x]`  | `{"url":"https://example.com","max_length":200,"raw":false}`                                                                                     |
| `docker_gateway__create_branch`                      | Git             | `[ ]`  | `{"branch":"feature-test","from_branch":"main","owner":"myorg","repo":"myrepo"}`                                                                 |
| `docker_gateway__create_entities`                    | Knowledge‑graph | `[ ]`  | `{"entities":[{"name":"entity1"}]}`                                                                                                              |
| `docker_gateway__create_or_update_file`              | Git             | `[ ]`  | `{"branch":"main","path":"README.md","content":"# test","message":"add test","owner":"myorg","repo":"myrepo"}`                                   |
| `docker_gateway__create_pending_pull_request_review` | GitHub          | `[ ]`  | `{"commitID":"abcd1234","owner":"myorg","pullNumber":1,"repo":"myrepo"}`                                                                         |
| `docker_gateway__create_pull_request`                | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","title":"test PR","body":"testing","head":"feature","base":"main","draft":false,"maintainer_can_modify":true}` |
| `docker_gateway__create_repository`                  | GitHub          | `[ ]`  | `{"name":"myrepo","description":"test repo","autoInit":true,"private":false,"organization":"myorg"}`                                             |
| `docker_gateway__create_relations`                   | Knowledge‑graph | `[ ]`  | `{"relations":[{"subject":"A","object":"B","predicate":"relates_to"}]}`                                                                          |
| `docker_gateway__delete_file`                        | Git             | `[ ]`  | `{"branch":"main","path":"README.md","message":"remove","owner":"myorg","repo":"myrepo"}`                                                        |
| `docker_gateway__get_commit`                         | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","sha":"abcd1234","include_diff":true}`                                                                         |
| `docker_gateway__get_file_contents`                  | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","path":"README.md","ref":"main"}`                                                                              |
| `docker_gateway__get_latest_release`                 | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo"}`                                                                                                              |
| `docker_gateway__list_branches`                      | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","page":1,"perPage":30}`                                                                                        |
| `docker_gateway__list_commits`                       | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","sha":"main","page":1,"perPage":30}`                                                                           |
| `docker_gateway__list_labels`                        | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo"}`                                                                                                              |
| `docker_gateway__list_issues`                        | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","state":"open","perPage":30}`                                                                                  |
| `docker_gateway__list_pull_requests`                 | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","state":"open","perPage":30}`                                                                                  |
| `docker_gateway__list_tags`                          | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","page":1,"perPage":30}`                                                                                        |
| `docker_gateway__merge_pull_request`                 | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","pullNumber":1,"commit_title":"Merge PR","commit_message":"Merging","merge_method":"merge"}`                   |
| `docker_gateway__request_copilot_review`             | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","pullNumber":1}`                                                                                               |
| `docker_gateway__search_code`                        | GitHub          | `[ ]`  | `{"query":"language:python","order":"desc","page":1,"perPage":30}`                                                                               |
| `docker_gateway__search_issues`                      | GitHub          | `[ ]`  | `{"query":"is:issue","owner":"myorg","repo":"myrepo"}`                                                                                           |
| `docker_gateway__search_nodes`                       | Knowledge‑graph | `[ ]`  | `{"query":"entity"}`                                                                                                                             |
| `docker_gateway__search_npm_packages`                | npm             | `[ ]`  | `{"searchTerm":"express","qualifiers":{}}`                                                                                                       |
| `docker_gateway__search_pull_requests`               | GitHub          | `[ ]`  | `{"query":"is:pr","owner":"myorg","repo":"myrepo"}`                                                                                              |
| `docker_gateway__search_repositories`                | GitHub          | `[ ]`  | `{"query":"topic:react","order":"desc","page":1,"perPage":30}`                                                                                   |
| `docker_gateway__search_users`                       | GitHub          | `[ ]`  | `{"query":"john smith","order":"desc","page":1,"perPage":30}`                                                                                    |
| `docker_gateway__update_issue`                       | GitHub          | `[ ]`  | `{"issue_number":1,"title":"New title","body":"detail","owner":"myorg","repo":"myrepo"}`                                                         |
| `docker_gateway__update_pull_request`                | GitHub          | `[ ]`  | `{"pullNumber":1,"title":"update title","owner":"myorg","repo":"myrepo","body":"update"}`                                                        |
| `docker_gateway__update_pull_request_branch`         | GitHub          | `[ ]`  | `{"pullNumber":1,"expectedHeadSha":"abcd1234","owner":"myorg","repo":"myrepo"}`                                                                  |
| `docker_gateway__get_tag`                            | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","tag":"v1.0"}`                                                                                                 |
| `docker_gateway__get_release_by_tag`                 | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","tag":"v1.0"}`                                                                                                 |
| `docker_gateway__get_me`                             | GitHub          | `[ ]`  | `{}`                                                                                                                                             |
| `docker_gateway__get_label`                          | GitHub          | `[ ]`  | `{"owner":"myorg","repo":"myrepo","name":"bug"}`                                                                                                 |
| `docker_gateway__open_nodes`                         | Knowledge‑graph | `[ ]`  | `{"names":["entity1","entity2"]}`                                                                                                                |
| `docker_gateway__read_graph`                         | Knowledge‑graph | `[ ]`  | `{}`                                                                                                                                             |
| `docker_gateway__delete_entities`                    | Knowledge‑graph | `[ ]`  | `{"entityNames":["old"]}`                                                                                                                        |
| `docker_gateway__delete_relations`                   | Knowledge‑graph | `[ ]`  | `{"relations":[{"subject":"A","object":"B","predicate":"relates_to"}]}`                                                                          |
| `docker_gateway__delete_observations`                | Knowledge‑graph | `[ ]`  | `{"deletions":[{"entity":"foo","value":"bar"}]}`                                                                                                 |
| `docker_gateway__execute_code`                       | Runtime         | `[ ]`  | `{"code":"console.log(\"hi\")","session_id":1}`                                                                                                  |
| `docker_gateway__sandbox_initialize`                 | Docker          | `[ ]`  | `{"image":"node:lts-slim","port":3000}`                                                                                                          |
| `docker_gateway__sandbox_exec`                       | Docker          | `[ ]`  | `{"commands":["echo hi"],"container_id":"123"}`                                                                                                  |
| `docker_gateway__sandbox_stop`                       | Docker          | `[ ]`  | `{"container_id":"123"}`                                                                                                                         |
| `docker_gateway__run_js_ephemeral`                   | Node            | `[ ]`  | `{"code":"console.log(\"hello\")","dependencies":[],"image":"node:lts-slim"}`                                                                    |
| `docker_gateway__run_js`                             | Node            | `[ ]`  | `{"code":"console.log(\"JS\")","container_id":"123","dependencies":[]}`                                                                          |
| `docker_gateway__puppeteer_click`                    | Browser         | `[ ]`  | `{"selector":"#submit"}`                                                                                                                         |
| `docker_gateway__puppeteer_evaluate`                 | Browser         | `[ ]`  | `{"script":"return document.title"}`                                                                                                             |
| `docker_gateway__puppeteer_fill`                     | Browser         | `[ ]`  | `{"selector":"input","value":"test"}`                                                                                                            |
| `docker_gateway__puppeteer_hover`                    | Browser         | `[ ]`  | `{"selector":"#hover"}`                                                                                                                          |
| `docker_gateway__puppeteer_navigate`                 | Browser         | `[ ]`  | `{"url":"https://example.com","allowDangerous":false}`                                                                                           |
| `docker_gateway__puppeteer_screenshot`               | Browser         | `[ ]`  | `{"name":"screen.png","selector":"#main","width":800,"height":600}`                                                                              |
| `docker_gateway__puppeteer_select`                   | Browser         | `[ ]`  | `{"selector":"select","value":"option2"}`                                                                                                        |
| `docker_gateway__get_dependency_types`               | npm             | `[ ]`  | `{"dependencies":[{"name":"express"}]}`                                                                                                          |
| `docker_gateway__fetch_content`                      | HTTP            | `[ ]`  | `{"url":"https://example.com"}`                                                                                                                  |
| `docker_gateway__search`                             | Search          | `[ ]`  | `{"query":"find something","max_results":10}`                                                                                                    |

---

The table above is intentionally *minimal* – only the essential fields are
shown.  If you wish to run a specific test, copy the example payload into
the CLI or your script and adjust any placeholder values.

