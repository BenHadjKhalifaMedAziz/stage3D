Room Generation Simulation
This simulation serves as a reusable component for other projects, particularly for dungeon generation or any game requiring dynamic room creation. The core idea is to allow developers to generate rooms of varying dimensions, defined by a user-provided range for height and width, as well as the total number of rooms.
Room Types and Numbers
The system categorizes rooms into three types:
Normal Rooms: These form the base structure and are defined by the user's input (e.g., if the user specifies 20 rooms, these are the primary ones to be generated).
Bonus Rooms: These are additional rooms with a probabilistic chance of being created, adding diversity to the structure.
Boss Room: This special room is derived from a normal room specified by the user. It is linked to its parent room while respecting specific constraints.
Despite the user defining a certain number of normal rooms, the final total will exceed this due to the addition of bonus and boss rooms. The system can handle a high number of rooms (e.g., up to 200) while maintaining structural and logical coherence.
Dynamic Room Generation
The system utilizes a gridless approach to generate rooms dynamically, without predefining a grid. This helps reduce memory usage, as demonstrated in tests consuming 5 GB of RAM for a 2D version.
Room Creation Process
Initial Room Creation: The process begins with the creation of the first room. Its dimensions are randomly selected within the user-defined range (min and max width/height). Cells for the room are generated dynamically and marked as reserved in a dictionary to prevent overlap or reuse.
Iterative Room Generation: After the first room is created, a random direction is chosen to attempt the placement of the next room. If the chosen direction does not have enough space to accommodate the room due to constraints or collisions, another direction is attempted. This process alternates between generating normal rooms and probabilistically creating bonus rooms connected to the most recently generated normal room.
Backtracking Logic: If a normal room cannot find space for the next room after exhausting all possible directions, the system backtracks to the previous room to check if a new direction from there can accommodate the room. If backtracking to the previous room also fails to find space, the system continues backtracking further up the chain of previously generated rooms. Bonus Rooms follow a similar backtracking logic.
Connection Logic
Walls are identified for each room based on its outermost cells. These walls are used to create connections between rooms by:
Highlighting walls on adjacent sides of connected rooms.
Creating doors at randomly selected cells on the overlapping walls, ensuring alignment.
Generating a passage between the doors, forming a bridge or hallway linking the rooms.
Wall Generation
Walls are generated based on the outer cells of each room, excluding corners to prevent overlap. Specific wall cells are assigned for prefabs and objects like doors or decorations, ensuring seamless integration with the environment.
Object Placement and Environment Creation
Each cell in a room carries details such as layer information, enabling hierarchical placement of objects:
Walls are on the outermost layer, while layers increase progressively toward the room's center.
Objects can be categorized and placed according to their context (e.g., near doors, in corners, on walls, or on floors).
Prefabs are organized within a nested dictionary structure:
The main dictionary holds room-level data.
Sub-dictionaries store cell-specific information, allowing precise placement of objects and decorations.
Optimization Considerations
The system prioritizes memory efficiency and performance:
Memory Management: A prebuilt grid was avoided to reduce memory consumption, as the large number of potential cells (especially with high room counts) would require significant resources. Instead, cells are created dynamically and only added to the dictionary when needed.
Performance: Using dictionaries for cell storage ensures faster lookups compared to lists, which would require iterating through all cells. This approach minimizes the computational load, even for extensive room generation scenarios involving hundreds of rooms.
Final Notes
This simulation ensures flexible, efficient, and scalable room generation. From normal to bonus and boss rooms, every detail walls, passages, and object placement respects defined constraints and logical connections. The use of a dynamic, dictionary-driven grid ensures a balance between memory efficiency and processing speed.
The screenshots provided demonstrate the 2D version's resource usage and the evolution of the room generation system.
