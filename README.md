# TiltFiveCalibrator
A simple reference calibrator for *perceived depth*.
Requires a wand for use of the Thumbstick and (1) button.

# Why does this exist? 
To help developers provide simple instructions / visuals to guide users through calibrating the Gameboard perceived depth. Hopefully this will be built into the SDK and solved permanently soon though.

# What does this Calibrate?
This tool provides a way of calibrating *perceived depth* to roughly match the *parallax plane* that rests at the board surface. 

This does not calibrate the positional error of the tracking, the accuracy of the board edge alignment or anything besides *perceived depth*.

# How does it change the perceived depth? 
The stereo pair (left / right) for each eye is shifted inward or outward away from your nose. The amount it is shifted in UV space is defined by the very small number in the lower right hand of the calibration screen.

# Images
![PXL_20220502_085253489](https://user-images.githubusercontent.com/3145170/166209581-74589e16-8e4d-40ca-b5dc-be6702989736.jpg)
![kLAqzFpYDA](https://user-images.githubusercontent.com/3145170/166209816-17d981ca-2cc1-45e4-a2e1-2c60feac158f.gif)

# Glossary
What does **perceived depth** mean? *Perceived depth* is the depth that your brain focuses and converges on visually. This is the depth you would place your finger on an object that is virtual or real. Ideally the virtual and physical, while side by side, should line up perfectly. Relates to [Depth Perception](https://www.aao.org/eye-health/anatomy/depth-perception)
