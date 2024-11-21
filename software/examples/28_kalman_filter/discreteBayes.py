import numpy as np
from filterpy.discrete_bayes import normalize, update

belief = np.array([1/10] * 10)
print("\nFirst belief")
print(belief)


hallway = np.array([1, 1, 0, 0, 0, 0, 0, 0, 1, 0])

print("\nhallway")
print (hallway)

belief = hallway * (1/3)

print("\nscaled belief")
print(belief)

def scaled_update(hall, belief, z, z_prob):
    scale = z_prob / (1. - z_prob)
    belief[hall == z] *= scale
    normalize(belief)

belief = np.array([0.1] * 10)
scaled_update(hallway, belief, z=1, z_prob=.75)

print('sum =', sum(belief))
print('probability of door =', belief[0])
print('probability of wall =', belief[2])



def lh_hallway(hall, z, z_prob):
    """ compute likelihood that a measurement matches
    positions in the hallway."""
    
    try:
        scale = z_prob / (1. - z_prob)
    except ZeroDivisionError:
        scale = 1e8

    likelihood = np.ones(len(hall))
    likelihood[hall==z] *= scale
    return likelihood

belief = np.array([0.1] * 10)
likelihood = lh_hallway(hallway, z=1, z_prob=.75)
posterior = update(likelihood, belief)
print("\nGeneralized method")
print(belief)
print(likelihood)
print(posterior)