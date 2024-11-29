# Room Generation Simulation

This simulation serves as a reusable component for other projects, particularly for dungeon generation or any game requiring dynamic room creation. It allows developers to generate rooms of varying dimensions, defined by a user-provided range for height and width, as well as the total number of rooms.

## Features

### Room Types and Numbers
The system categorizes rooms into three types:
- **Normal Rooms**: These form the base structure, defined by the user's input (e.g., if the user specifies 20 rooms, these are the primary ones to be generated).
- **Bonus Rooms**: Additional rooms with a probabilistic chance of being created, adding diversity to the structure.
- **Boss Room**: A special room derived from a normal room specified by the user. It is linked to its parent room while respecting specific constraints.

Despite the user specifying a certain number of normal rooms, the final total may exceed this due to the inclusion of bonus and boss rooms. The system can handle a high number of rooms (e.g., up to 200) while maintaining structural and logical coherence.

### Dynamic Room Generation
The system employs a gridless approach for room generation, reducing memory usage and improving performance. Tests for the 2D version demonstrated memory consumption of approximately 5 GB.

## Room Creation Process

1. **Initial Room Creation**  
   - Starts with the first room, whose dimensions are randomly selected within the user-defined range (min/max width and height).
   - Cells for the room are generated dynamically and marked as reserved in a dictionary to prevent overlap or reuse.

2. **Iterative Room Generation**  
   - A random direction is chosen to attempt the placement of the next room. If the direction lacks space due to constraints or collisions, another direction is tried.
   - Alternates between generating normal rooms and probabilistically creating bonus rooms connected to the most recently generated normal room.

3. **Backtracking Logic**  
   - If a normal room cannot find space after exhausting all directions, the system backtracks to the previous room to explore new directions.
   - Backtracking continues up the chain of previously generated rooms if necessary. Bonus rooms follow a similar backtracking process.

## Connection Logic

- **Wall Identification**: Walls are determined based on the outermost cells of each room.
- **Creating Connections**:  
  - Overlapping walls are highlighted for connected rooms.  
  - Doors are placed at randomly selected cells on overlapping walls, ensuring alignment.  
  - A passage is created between doors to form a bridge or hallway.

## Wall Generation

Walls are generated from the outer cells of each room, excluding corners to prevent overlap. Specific wall cells are designated for prefabs and objects like doors or decorations, ensuring seamless integration with the environment.

## Object Placement and Environment Creation

Each cell in a room contains layer information for hierarchical object placement:
- **Walls**: Outer layer.
- **Progressive Layers**: Increase toward the center of the room.
- **Object Categories**: Include doors, corners, walls, and floor objects.

**Prefabs** are organized within a nested dictionary structure:
- The main dictionary contains room-level data.
- Sub-dictionaries store cell-specific information for precise object and decoration placement.

## Optimization Considerations

- **Memory Management**:  
  - Avoids prebuilt grids to reduce memory consumption. Cells are created dynamically and added to a dictionary only when needed.
- **Performance**:  
  - Uses dictionaries for faster cell lookups, avoiding iterative overhead associated with lists.  
  - Ensures efficient computation even with extensive room generation involving hundreds of rooms.
 
    <img width="452" alt="OLD VERSION RAM" src="https://github.com/user-attachments/assets/8bb024fc-636c-4aab-9e36-3ae0112216b2">
<img width="715" alt="opt" src="https://github.com/user-attachments/assets/8a17865b-1736-4a1a-bcac-98b4a828dbcb">

<img width="1139" alt="popt1" src="https://github.com/user-attachments/assets/45784d52-6fdd-4960-a16d-138865c74296">
<img width="654" alt="popt2" src="https://github.com/user-attachments/assets/9537f506-b331-4477-a9a1-c81477ec666c">

## Final Notes

This simulation provides flexible, efficient, and scalable room generation. From normal to bonus and boss rooms, every detail—walls, passages, and object placement—respects defined constraints and logical connections. The dynamic, dictionary-driven grid ensures a balance between memory efficiency and processing speed.

## Screenshots


The following screenshots demonstrate the 2D version's resource usage and the evolution of the room generation system:

1. **First Version:**  
![YU-CFaZx](https://github.com/user-attachments/assets/3ccae2b8-ccee-4442-abb1-10d0da8901f3)

2. **Version 2: Wall ,Doors and Passages Creation**  
<img width="394" alt="2" src="https://github.com/user-attachments/assets/e1cb71fe-5508-408b-b868-f35d7e2ba23e">
3. **Showcase: Unlimited Rooms (400 Rooms)**  
<img width="371" alt="3" src="https://github.com/user-attachments/assets/2b815c9e-ab7a-4542-a4da-2c16265b7dc0">
4. **3D Version 1** 
 <img width="401" alt="Screenshot 2024-11-29 230354" src="https://github.com/user-attachments/assets/2821433e-3d02-4183-a307-a44fea1740fc">
5. **3D Version 1** 
<img width="1089" alt="last V" src="https://github.com/user-attachments/assets/f8b0a9c7-a147-4a92-bbde-53cd8cb0d46e">
6. **Adding Real Walls and Planes**  
<img width="1280" alt="300room" src="https://github.com/user-attachments/assets/fc301ef0-0955-48e1-be66-db83d7097fe9">

2. ****  
 





3. ****  
  
---

We welcome contributions and suggestions to improve the Room Generation Simulation. Feel free to fork this repository and share your ideas!
