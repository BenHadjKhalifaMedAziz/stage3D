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

## Final Notes

This simulation provides flexible, efficient, and scalable room generation. From normal to bonus and boss rooms, every detail—walls, passages, and object placement—respects defined constraints and logical connections. The dynamic, dictionary-driven grid ensures a balance between memory efficiency and processing speed.

## Screenshots


The following screenshots demonstrate the 2D version's resource usage and the evolution of the room generation system:
1. ****  ![YU-CFaZx](https://github.com/user-attachments/assets/2302f825-d00a-4940-be19-457029f513bf)

   ![FElCT28v](https://github.com/user-attachments/assets/994e97c5-0317-4ba5-a354-76adceba00f6)
![AskuvEu9](https://github.com/user-attachments/assets/adaa1713-16b6-4bf6-b85a-010e7987dbee)
![Yr88QcsX](https://github.com/user-attachments/assets/dc5f71a7-0e1d-42fc-b40c-cb6cb59375a0)
![uE4izatg](https://github.com/user-attachments/assets/9740b35d-b426-4217-87e4-e4800c06a91b)
<img width="1089" alt="last V" src="https://github.com/user-attachments/assets/a0afc6de-3b92-4135-8b1e-b2511a9d793c">

<img width="1280" alt="300room" src="https://github.com/user-attachments/assets/697b4f76-9c52-450c-b2e4-be52ef0e7b8f">

2. ****  
   ![Room Connections](https://your-image-url.com/room-connections.png)





3. ****  
   ![Bonus and Boss Rooms](https://your-image-url.com/bonus-boss-rooms.png)

---

We welcome contributions and suggestions to improve the Room Generation Simulation. Feel free to fork this repository and share your ideas!
