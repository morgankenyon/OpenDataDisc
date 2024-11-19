# Kalman Filter Tutorial

I've been utilizing Claude to try to understand how to implement a Kalman filter.

At the moment I've been unable to determine where my problems lie, and I don't know enough about the Kalman filter to actually understand what to change. So taking a step back and going to write something from scratch.

## Issues

### Cannot find ipywidgets.
```
ModuleNotFoundError                       Traceback (most recent call last)
<ipython-input-2-44ce08f6fafe> in <module>
      1 #format the book
----> 2 import book_format
      3 book_format.set_style()

C:\dev\Kalman-and-Bayesian-Filters-in-Python\book_format.py in <module>
     20 import sys
     21 import warnings
---> 22 from kf_book.book_plots import set_figsize, reset_figsize
     23 
     24 def test_installation():

C:\dev\Kalman-and-Bayesian-Filters-in-Python\kf_book\book_plots.py in <module>
     18 
     19 from contextlib import contextmanager
---> 20 import ipywidgets
     21 import matplotlib as mpl
     22 import matplotlib.pylab as pylab

ModuleNotFoundError: No module named 'ipywidgets'
```


Running `jupyter --version` gave me the following output:
```
PS C:\Users\morga> jupyter --version
jupyter core     : 4.6.3
jupyter-notebook : 6.0.3
qtconsole        : not installed
ipython          : 7.13.0
ipykernel        : 5.1.4
jupyter client   : 6.0.0
jupyter lab      : 2.0.1
nbconvert        : 5.6.1
ipywidgets       : not installed
nbformat         : 5.0.4
traitlets        : 4.3.3
```

https://ipywidgets.readthedocs.io/en/latest/user_install.html

Root issue: I changed to use the version of python that came with Anaconda versus what I had previously installed on my machine.

Resolution:

* Added the following items to my path variable:
  * `C:\Users\{username}\anaconda3`
  * `C:\Users\{username}\anaconda3\Scripts`
* place them higher than any current python references

![alt text](image.png)


Afterwards, I received the following when running `jupyter --version`:

```
PS C:\Users\morga> jupyter --version
Selected Jupyter core packages...
IPython          : 8.27.0
ipykernel        : 6.28.0
ipywidgets       : 8.1.5
jupyter_client   : 8.6.0
jupyter_core     : 5.7.2
jupyter_server   : 2.14.1
jupyterlab       : 4.2.5
nbclient         : 0.8.0
nbconvert        : 7.16.4
nbformat         : 5.10.4
notebook         : 7.2.2
qtconsole        : 5.5.1
traitlets        : 5.14.3
PS C:\Users\morga>
```

Stackoverflow answer: https://stackoverflow.com/a/62689395/1352766

### Cannot find filterpy

```
Please install FilterPy from the command line by running the command
	$ pip install filterpy

See chapter 0 for instructions.
UnboundLocalError                         Traceback (most recent call last)
Cell In[4], line 2
      1 #format the book
----> 2 import book_format
      3 book_format.set_style()

File C:\dev\Kalman-and-Bayesian-Filters-in-Python\book_format.py:80
     71         print('You must use Python version 3.6 or later for the notebooks to work correctly')
     74     # need to add test for IPython. I think I want to be at 6, which also implies
     75     # Python 3, matplotlib 2+, etc.
     76 
     77 # ensure that we have the correct packages loaded. This is
     78 # called when this module is imported at the top of each book
     79 # chapter so the reader can see that they need to update their environment.
---> 80 test_installation()
     83 # now that we've tested the existence of all packages go ahead and import
     85 import matplotlib

File C:\dev\Kalman-and-Bayesian-Filters-in-Python\book_format.py:52, in test_installation()
     48     print("Please install matplotlib before continuing. See chapter 0 for instructions.")
     50 from distutils.version import LooseVersion
---> 52 v = filterpy.__version__
     53 min_version = "1.4.4"
     54 if LooseVersion(v) < LooseVersion(min_version):

UnboundLocalError: cannot access local variable 'filterpy' where it is not associated with a value
```

I've installed it via pip, trying to see if I need to install it via conda.

I ran `conda install filterpy` to install it and it seemed to work.

After installing conda, it seems like I have two versions of python. So I got to make sure everything is installed on the right location.

### Plots Not Displaying due to Jupyter Not Trusting Notebook

For a while I couldn't get the plots displaying.

I had to run the following for each notebook:

`jupyter trust .\01-g-h-filter.ipynb`

That allowed Jupyter to trust each notebook and allowed them to display the plots.