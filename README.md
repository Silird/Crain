# Crain

Crain is the in-game script for Space Engineers that allow you to control construction with 3 pistons and 2 rotors seating in cocpit with WASD(E, Q, C, Space) and mouse.

## Features

- All kind of work depend on the tool installed on crain (weld, grind, drill).
- Using conveyor system of your base/ship.
- Many degrees of freedom.

Scheme of crain:
```
                  Piston X
       Rotor->[*]---------->[*]<-Hinge
               ^             |  
    Piston Z   |             |   Piston X
               |            [#]<-Tool
------------->[ ]
 Piston Base
 ```
 
I advice to install remote control block to control crain, you can install cocpit and setup "control remote control" to hotkey and operate remote control in cocpit. Regardless, you can still use the cockpit as your primary control source, just name it appropriately.
All crain block will have suffix which is specified in the variable *shipName*. This is necessary if several cranes will be installed on the same grid.
 
- Crain control block (cocpit or remote control)
**Remote Control shipName**
- Rotor of the crane movement around its axis
**Rotor shipName**
Mouse control left/right
- Hinge of the crane movement up/down
**Hinge shipName**
Mouse control up/down
- Left/Right Piston:
**Piston X shipName**
A/D control
- Forward/Backward Piston:
**Piston Y shipName**
W/S control
- Up/Down Piston:
**Piston Z shipName**
Space/C control
- Base movement Piston:
**Piston Base shipName**
Q/E control

If rotors or pistons move in wrong direction just invert match value starts with *ivert*. 

**rotationCoef** - rotation coefficient of the rotors witch controls by mouse. The higher the value, the faster the movement.
**pistonVelocity** - piston velocity in crain from 0 to 5.