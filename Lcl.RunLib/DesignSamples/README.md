# Design Samples
This folder contains some prototype run configuration files,
to help guide the design. It also contains some real world examples.

## Examples

### Example 1: A python virtual environment

The `pyenv-3-10.venv.appdef2.json` example runs python in a 
particular virtual environment. In this case it is a "normal" 
virtual environment, not an Anaconda one.
It can be run stand-alone to start an interactive python 
session in this virtual environment by invoking

```bat
lcrun pyenv-3-10.venv
```

Or you can use it to run python scripts in this environment. 
For example, to run pip and list the installed packages:

```bat
lcrun pyenv-3-10.venv -m pip list
```

Here is the content of this appdef:

```json
{
  "description": "Python 3.10 Standard virtual environment",
  "base": null,
  "tobase": {},
  "frombase": {
    "command": "C:\\Users\\ttelcl\\.venv\\pyenv-3-10\\Scripts\\python.exe",
    "vars": {
      "VIRTUAL_ENV": "C:\\Users\\ttelcl\\.venv\\pyenv-3-10",
      "VIRTUAL_ENV_PROMPT": "(pyenv-3-10)",
      "PROMPT": "(pyenv-3-10) $P$G"
    }
  }
}
```

A few observations:

* There is no need to modify the path explicitly, like the actual
  virtual environment activation script does: the directory where
  the command to execute resides is automatically prepended by
  `lcrun`, unless you opt out of that feature explicitly
* Since this appdef has no base, it doesn't really matter if you
  use the `tobase` or the `frombase` section. By convention 
  use the `frombase` one.
* The appdef's name is `pyenv-3-10.venv`. Note the use of a
  pseudo-extension `.venv` to hint that this a (python) _virtual 
  environment_ definition, and is intended to be used as base
  appdef in other appdefs.

### Example 2: A python application

The `pip-3-10.appdef2.json` is an example of a _layered_ 
application definition file, to run a python application (the
standard python _pip_ package manager tool) in the
virtual environment that is set up in the base appdef (in this
case that is the abovementioned `pyenv-3-10`). To run the
same command as was shown at the end of the previous example
you can run:

```batch
lcrun pip-3-10 list
```

Here is the content of this appdef:

```json
{
  "description": "run PIP in the pyenv-3-10 virtual environment",
  "base": "pyenv-3-10.venv",
  "tobase": {},
  "frombase": {
    "args": {
      "prepend": [
        "-m",
        "pip"
      ]
    }
  }
}
```

Observations and notes:
* Note there is no "command" here; that is the base appdef's
  responsibility.
* In this case there are no things affected by both the base
  and this appdef. Therefore there is no effective difference
  between the `tobase` and `frombase` sections


