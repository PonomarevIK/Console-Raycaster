using static System.Math;

const int screenWidth = 120;    // default console size is 120 characters wide
const int screenHeight = 30;    //                      and 30 character high
const int unit = 64;            // arbitrary unit of distance equal to one square on the map
const int mapSize = 8;

int[,] map = {
    {1,1,1,1,1,1,1,1},
    {1,0,1,0,0,0,0,1},
    {1,0,1,0,0,0,0,1},
    {1,0,1,0,0,0,0,1},
    {1,0,0,0,0,0,0,1},
    {1,0,0,0,0,1,0,1},
    {1,0,0,0,0,0,0,1},
    {1,1,1,1,1,1,1,1}};
string[] walls = new string[screenWidth];


double DegToRad(double a) => a * PI / 180.0;


double LimitAngle(double a) 
{
    if (a >= 359) return a - 360;
    if (a < 0) return a + 360;
    return a;
}


// Returns a string of 'height' number of block characters (░ ▒ ▓ █ ▀ ▄) padded on both sides to have total length of 'screenHeight'
static string WallOfHeight(int height, char wall = '█', char floor = '░', char ceiling = ' ')
{
    if (height >= screenHeight * 2)
    {
        // Height can not exceed max screen height
        return new string(wall, screenHeight);
    }
    if (height < 0) 
    {
        // Height cannot be negative so something went wrong
        return new string('?', screenHeight);  
    }

    int half = height / 2;
    string topHalf = (new string('▄', half % 2) + new string(wall, half / 2)).PadLeft(screenHeight / 2, ceiling);
    string bottomHalf = (new string(wall, half / 2) + new string('▀', half % 2)).PadRight(screenHeight / 2, floor);

    return topHalf + bottomHalf;
}


int ray, mx, my, depth;
double rayX, rayY, rayAngle, dist, distV, distH, tg, lineHeight;
char wall;

double camAngle = 90;
double camX = 200, 
       camY = 400; 
double pdx =  Cos(DegToRad(camAngle)),
       pdy = -Sin(DegToRad(camAngle));
double offsetX = 0,
       offsetY = 0;


// MAIN LOOP
while (true)
{
    Console.Clear();
    rayAngle = camAngle + 30;

    // Cast 120 rays 0.5 degrees apart
    for (ray = 0; ray < screenWidth; ray++, rayAngle -= 0.5)
    {
        // ----Looking for the closest VERTICAL wall----
        depth = 0; 
        distV = mapSize * unit;
        tg = Tan(DegToRad(rayAngle));

        if (Cos(DegToRad(rayAngle)) > 0)               // looking right
        {
            rayX = ((int)camX / unit + 1) * unit;
            rayY = (camX - rayX) * tg + camY; 
            offsetX = unit; 
            offsetY = -offsetX * tg; 
        }
        else if (Cos(DegToRad(rayAngle)) < 0)          // looking left
        {
            rayX = ((int)camX / unit * unit) - 0.0001;
            rayY = (camX - rayX) * tg + camY; 
            offsetX = -unit; 
            offsetY = -offsetX * tg; 
        }
        else                                           // looking straight vertically, cannot hit a vertical wall
        { 
            rayX = camX; 
            rayY = camY; 
            depth = mapSize; 
        }                                                  

        for (; depth < mapSize; depth++)
        {
            mx = (int)rayX / unit; 
            my = (int)rayY / unit;
            if (mx >= 0 && my >= 0 && mx < mapSize && my < mapSize && map[mx, my] == 1)            // ray hit a vertical wall
            { 
                distV = Cos(DegToRad(rayAngle)) * (rayX - camX) - Sin(DegToRad(rayAngle)) * (rayY - camY); 
                break; 
            }
            else        //check next line
            { 
                rayX += offsetX; 
                rayY += offsetY;
            }                                               
        }

        // ----Looking for the closest HORIZONTAL wall----
        depth = 0; 
        distH = mapSize * unit;
        tg = 1.0 / tg;

        if (Sin(DegToRad(rayAngle)) > 0)               // looking up  
        {
            rayY = ((int)camY / unit * unit) - 0.0001;
            rayX = (camY - rayY) * tg + camX; 
            offsetY = -64; 
            offsetX = -offsetY * tg; 
        }
        else if (Sin(DegToRad(rayAngle)) < 0)          // looking down
        {
            rayY = ((int)camY / unit + 1) * unit;
            rayX = (camY - rayY) * tg + camX;
            offsetY = unit; 
            offsetX = -offsetY * tg; 
        }
        else                                           // looking straight horizontally, cannot hit a horizontal wall
        { 
            rayX = camX; 
            rayY = camY; 
            depth = mapSize; 
        }                                                   

        for(; depth < mapSize; depth++)
        {
            mx = (int)rayX / unit; 
            my = (int)rayY / unit;
            if (mx >= 0 && my >= 0 && mx < mapSize && my < mapSize && map[mx, my] == 1)            // ray hit a horizontal wall
            {  
                distH = Cos(DegToRad(rayAngle)) * (rayX - camX) - Sin(DegToRad(rayAngle)) * (rayY - camY); 
                break; 
            }       
            else        // check next line
            { 
                rayX += offsetX; 
                rayY += offsetY;
            }                                        
        }

        if (distV < distH) { dist = distV; wall = '▓'; }         // vertical walls are a slightly lighter shade
        else               { dist = distH; wall = '█'; }

        dist *= Cos(DegToRad(camAngle - rayAngle));              // fix fisheye effect
        lineHeight = (unit * screenHeight * 2) / dist;

        walls[ray] = WallOfHeight((int)lineHeight, wall);
    }

    // Rendering the resulting image in console line by line
    for (int i = 0; i < screenHeight; i++)
    {
        for (int j = 0; j < screenWidth; j++)
        {
            Console.Write(walls[j][i]);
        }
        if (i != screenHeight - 1) Console.WriteLine();
    }

    // Key input to move/look around
    var key = Console.ReadKey(false).Key;
    if (key == ConsoleKey.UpArrow)               // move forward
    {
        camX += pdx * 10;
        camY += pdy * 10;
        if (map[(int)camX / unit, (int)camY / unit] == 1)  // prevent going through walls
        {
            camX -= pdx * 10;
            camY -= pdy * 10;
        }
    }
    else if (key == ConsoleKey.DownArrow)        // move backwards
    {
        camX -= pdx * 10;
        camY -= pdy * 10;
        if (map[(int)camX / unit, (int)camY / unit] == 1)  // prevent going through walls
        {
            camX += pdx * 10;
            camY += pdy * 10;
        }
    }
    else if (key == ConsoleKey.LeftArrow)        // turn 10 deg. left
    {
        camAngle = LimitAngle(camAngle + 10);
        pdx =  Cos(DegToRad(camAngle));
        pdy = -Sin(DegToRad(camAngle));
    }
    else if (key == ConsoleKey.RightArrow)       // turn 10 deg. right
    {
        camAngle = LimitAngle(camAngle - 10);
        pdx =  Cos(DegToRad(camAngle));
        pdy = -Sin(DegToRad(camAngle));
    }
    else                                         // exit program
    {
        break; // <- this is why I had to use if-else instead of switch-case
    }
}



