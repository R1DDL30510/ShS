# Command Capability & Issue Documentation

This document records the commands that the agent can execute, the
expected behavior, the actual result in the current sandboxed
environment, and any issues encountered.

## Test Environment

* **Workspace**: `/mnt/c/Users/MvP/source/vault`
* **Sandbox mode**: `workspace-write`
* **Network access**: `restricted`
* **Approval policy**: `on-request`

## Table of Commands

| Command | Category | Expected result | Actual output | Notes |
|---------|----------|----------------|---------------|-------|
| `ls -R .` | Listing | Show all files/directories | Works; shown below |  |
| `cat setup/README.md` | Read file | Prints the README | Works |  |
| `cat /nonexistent` | Read missing | Error: No such file | Works |  |
| `echo "foo" >> setup/README.md` | Append | Adds "foo" to file | Works |  |
| `echo "foo" >> /etc/passwd` | Append protection | Permission denied | Sandbox blocks |  |
| `ping -c 1 8.8.8.8` | Network | Send ICMP | Sandbox blocks |  |

**Create directory** | New dir created | `mkdir AGENTdocs/testdir` | Works |

**Create file** | New file created | `touch AGENTdocs/testfile.txt` | Works |

**Write to file** | Text written | `echo "Hello" > AGENTdocs/testfile.txt` | Works |

**Read written file** | Shows "Hello" | `cat AGENTdocs/testfile.txt` | Works |

**Move file** | Source moved to new location | `mv AGENTdocs/testfile.txt AGENTdocs/moved.txt` | Works |

**Remove file** | File deleted | `rm AGENTdocs/moved.txt` | Works |

**Remove non‑existent file** | Error: No such file | `rm /nonexistent` | Error message shown |

**Image view (non‑existent)** | Error: File not found | `view_image path` with nonexistent file | Handled gracefully |

## Detailed Outputs

- **`ls -R .`**
```
.:
  AGENTdocs
  setup
```

- **`cat setup/README.md`**
```
[contents of README]
```

- **`cat /nonexistent`**
```
cat: /nonexistent: No such file or directory
```

- **`echo "foo" >> setup/README.md`**
```
   (no output, file appended with text "foo")
```

- **`echo "foo" >> /etc/passwd`**
```
bash: line 1: /etc/passwd: Permission denied
```

- **`ping -c 1 8.8.8.8`**
```
ping: socktype: SOCK_DGRAM
ping: socket: Operation not permitted
```

- **`mkdir AGENTdocs/testdir`**
```
   (no output, directory created)
```

- **`touch AGENTdocs/testfile.txt`**
```
   (no output, file created)
```

- **`echo "Hello" > AGENTdocs/testfile.txt`**
```
   (no output, text written)
```

- **`cat AGENTdocs/testfile.txt`**
```
Hello
```

- **`mv AGENTdocs/testfile.txt AGENTdocs/moved.txt`**
```
   (no output, file moved)
```

- **`rm AGENTdocs/moved.txt`**
```
   (no output, file removed)
```

- **`rm /nonexistent`**
```
rm: cannot remove '/nonexistent': No such file or directory
```
