using System.Text.Json;
using Harmonizer.Models;
using Microsoft.EntityFrameworkCore;

namespace Harmonizer.Data;

public static class DataSeeder
{
    public static async Task SeedData(HarmonizerDbContext context)
    {
        // Seed multi-tenancy hierarchy first
        await SeedMultiTenancy(context);
        
        // Seed Prompts
        await SeedPrompts(context);
        
        // Seed Workflows
        await SeedWorkflows(context);
        
        // Create dynamic tables and seed sample data
        await SeedDynamicTablesAndData(context);
        
        // Only seed requests if database is empty
        if (context.HarmonizeRequests.Any())
        {
            return;
        }

        var requests = GenerateSampleRequests();
        
        await context.HarmonizeRequests.AddRangeAsync(requests);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMultiTenancy(HarmonizerDbContext context)
    {
        if (context.Tenants.Any())
        {
            return;
        }

        // Create sample tenant hierarchy
        var acmeTenant = new Tenant
        {
            Name = "Acme Corporation",
            Slug = "acme",
            Description = "Demo manufacturing company",
            IsActive = true
        };
        context.Tenants.Add(acmeTenant);
        await context.SaveChangesAsync();

        // Production namespace
        var prodNamespace = new Namespace
        {
            Name = "Production",
            Slug = "production",
            Description = "Production environment data",
            TenantId = acmeTenant.Id,
            IsActive = true
        };
        
        // Staging namespace
        var stagingNamespace = new Namespace
        {
            Name = "Staging",
            Slug = "staging",
            Description = "Staging/testing environment",
            TenantId = acmeTenant.Id,
            IsActive = true
        };
        
        context.Namespaces.AddRange(prodNamespace, stagingNamespace);
        await context.SaveChangesAsync();

        // Teams under Production
        var iotTeam = new Team
        {
            Name = "IoT Platform",
            Slug = "iot",
            Description = "Sensor and IoT device data",
            NamespaceId = prodNamespace.Id,
            IsActive = true
        };
        
        var mfgTeam = new Team
        {
            Name = "Manufacturing",
            Slug = "manufacturing",
            Description = "Machine and production data",
            NamespaceId = prodNamespace.Id,
            IsActive = true
        };
        
        var qualityTeam = new Team
        {
            Name = "Quality Control",
            Slug = "quality",
            Description = "Quality inspection data",
            NamespaceId = prodNamespace.Id,
            IsActive = true
        };
        
        // Team under Staging
        var testTeam = new Team
        {
            Name = "Test Team",
            Slug = "test",
            Description = "For testing workflows",
            NamespaceId = stagingNamespace.Id,
            IsActive = true
        };
        
        context.Teams.AddRange(iotTeam, mfgTeam, qualityTeam, testTeam);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPrompts(HarmonizerDbContext context)
    {
        if (context.Prompts.Any())
        {
            return;
        }

        var prompts = new List<Prompt>
        {
            new Prompt
            {
                Name = "Industry 4.0 - Unified Equipment Format",
                Description = "Transform diverse machine data into standardized Industrial IoT format",
                DesiredOutputType = "JSON",
                ContextPrompt = @"You are an Industry 4.0 data harmonization engine. Transform the provided machine/equipment data into a standardized Industrial IoT format.

HARMONIZATION OBJECTIVES:
1. Normalize diverse equipment data into a unified schema
2. Extract key operational metrics and KPIs
3. Standardize status codes and classifications
4. Preserve critical telemetry and sensor data
5. Enable cross-equipment analytics and monitoring

TRANSFORMATION RULES:
- equipment.id <- extract from machineId, robotId, systemId, vehicleId, etc.
- equipment.type <- normalize equipment type (CNC_MACHINE, ROBOT, AGV, 3D_PRINTER, CONVEYOR, etc.)
- equipment.manufacturer & equipment.model <- preserve as-is
- equipment.location <- standardize location hierarchy (facility > area > zone > cell)
- timestamp <- convert to ISO 8601 UTC format
- status.operational <- map to: RUNNING, IDLE, MAINTENANCE, ERROR, OFFLINE
- status.mode <- map to: AUTO, MANUAL, SETUP, TEST
- status.health <- calculate 0-100 score based on efficiency, uptime, error rates
- performance.efficiency <- extract or calculate overall equipment efficiency (OEE)
- performance.uptime <- extract or calculate uptime percentage
- performance.cycleTime <- actual vs target cycle time
- performance.throughput <- units/hour or appropriate rate metric
- metrics.production <- parts produced, completed cycles, etc.
- metrics.quality <- quality scores, defect rates, pass rates
- telemetry <- extract all sensor readings (temperature, pressure, vibration, etc.)
- telemetry.alerts <- extract any warnings or alerts with severity levels
- maintenance.hours <- operating hours, runtime
- maintenance.nextService <- next scheduled maintenance date
- energy.powerConsumption <- current power draw in kW
- process <- capture active job, program, recipe, batch information
- audit.source <- preserve original system name
- audit.harmonizedAt <- current timestamp

OUTPUT: Generate ONLY valid JSON matching the desired schema. Use camelCase consistently.",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    harmonizedData = new
                    {
                        equipment = new
                        {
                            id = "",
                            type = "",
                            manufacturer = "",
                            model = "",
                            location = new { facility = "", area = "", zone = "", cell = "" }
                        },
                        timestamp = "",
                        status = new { operational = "", mode = "", health = 0, currentProgram = "" },
                        performance = new
                        {
                            efficiency = 0.0,
                            uptime = 0.0,
                            cycleTime = new { actual = 0.0, target = 0.0 },
                            throughput = 0.0
                        },
                        metrics = new
                        {
                            production = new { completed = 0, target = 0, percentComplete = 0.0 },
                            quality = new { score = 0.0, defectRate = 0.0, passRate = 0.0 }
                        },
                        telemetry = new Dictionary<string, object>(),
                        alerts = new[] { new { id = "", severity = "", message = "", timestamp = "" } },
                        maintenance = new
                        {
                            operatingHours = 0.0,
                            nextService = "",
                            healthIndicators = new Dictionary<string, object>()
                        },
                        energy = new { powerConsumption = 0.0, energyUnit = "kW" },
                        process = new { jobId = "", material = "", parameters = new Dictionary<string, object>() },
                        audit = new
                        {
                            source = "",
                            originalRecordType = "",
                            harmonizedAt = "",
                            schemaVersion = "Industry4.0-v1.0"
                        }
                    }
                }, new JsonSerializerOptions { WriteIndented = true })
            },
            new Prompt
            {
                Name = "CNC Machine - Production Report",
                Description = "Extract production metrics from CNC machine data",
                DesiredOutputType = "JSON",
                ContextPrompt = @"Transform this CNC machine data into a production report format. Extract:
- Machine identification and status
- Production metrics (parts produced, efficiency, cycle times)
- Spindle and tool information
- Quality indicators
- Maintenance status

Focus on operational KPIs and production performance.",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    report = new
                    {
                        machine = new { id = "", type = "CNC", status = "" },
                        production = new { partsProduced = 0, target = 0, efficiency = 0.0, cycleTime = 0.0 },
                        spindle = new { speed = 0, load = 0.0, temperature = 0.0 },
                        quality = new { defectRate = 0.0, inSpec = true },
                        maintenance = new { hoursUntilService = 0.0, status = "OK" }
                    }
                }, new JsonSerializerOptions { WriteIndented = true })
            },
            new Prompt
            {
                Name = "Robot - Welding Quality Report",
                Description = "Extract welding quality metrics from robot data",
                DesiredOutputType = "JSON",
                ContextPrompt = @"Analyze this industrial robot welding data and create a quality report. Extract:
- Welding parameters (current, voltage, wire speed)
- Quality metrics (weld quality score, defect count, rework rate)
- Robot health indicators (joint temperatures, overall health)
- Cycle performance

Highlight any quality concerns or deviations from standards.",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    weldingReport = new
                    {
                        robot = new { id = "", status = "", program = "" },
                        weldingParams = new { current = 0, voltage = 0.0, wireSpeed = 0.0, gasFlow = 0.0 },
                        quality = new
                        {
                            weldQuality = 0.0,
                            penetrationDepth = 0.0,
                            defectCount = 0,
                            reworkRate = 0.0
                        },
                        robotHealth = new { overallHealth = 0.0, jointTemps = new int[0] },
                        performance = new { cyclesCompleted = 0, cycleTime = 0.0 }
                    }
                }, new JsonSerializerOptions { WriteIndented = true })
            },
            new Prompt
            {
                Name = "AGV - Fleet Status Summary",
                Description = "Summarize AGV mission and health status",
                DesiredOutputType = "JSON",
                ContextPrompt = @"Create an AGV fleet status summary from this vehicle data. Include:
- Current position and mission status
- Battery health and charge level
- Navigation performance
- Task completion progress

Provide actionable insights for fleet management.",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    agvStatus = new
                    {
                        vehicle = new { id = "", type = "AGV" },
                        position = new { x = 0.0, y = 0.0, zone = "", heading = 0 },
                        mission = new { taskId = "", status = "", progress = 0, eta = "" },
                        battery = new { level = 0, voltage = 0.0, estimatedRange = 0.0 },
                        navigation = new { speed = 0.0, mode = "", obstaclesDetected = 0 }
                    }
                }, new JsonSerializerOptions { WriteIndented = true })
            },
            new Prompt
            {
                Name = "Quality Inspection - Defect Analysis",
                Description = "Analyze quality inspection data for defect patterns",
                DesiredOutputType = "JSON",
                ContextPrompt = @"Analyze this quality inspection system data and generate a defect analysis report. Extract:
- Inspection metrics (parts inspected, pass/fail rates)
- Defect categorization and counts
- Quality trends and patterns
- System calibration status

Identify the most common defect types and their frequencies.",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    qualityReport = new
                    {
                        inspection = new { system = "", recipe = "", partsInspected = 0 },
                        results = new { passRate = 0.0, rejectRate = 0.0, partsRejected = 0 },
                        defects = new
                        {
                            dimensional = 0,
                            surface = 0,
                            missing = 0,
                            orientation = 0,
                            total = 0
                        },
                        performance = new { inspectionTime = 0.0, accuracy = 0.0 },
                        calibration = new { lastCalibration = "", accuracy = 0.0 }
                    }
                }, new JsonSerializerOptions { WriteIndented = true })
            }
        };

        await context.Prompts.AddRangeAsync(prompts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedWorkflows(HarmonizerDbContext context)
    {
        if (context.Workflows.Any())
        {
            return;
        }

        // Get teams for assignment
        var iotTeam = await context.Teams.FirstOrDefaultAsync(t => t.Slug == "iot");
        var mfgTeam = await context.Teams.FirstOrDefaultAsync(t => t.Slug == "manufacturing");
        var qualityTeam = await context.Teams.FirstOrDefaultAsync(t => t.Slug == "quality");

        var workflows = new List<Workflow>
        {
            new Workflow
            {
                Name = "IoT Sensor Data Processor",
                Description = "Harmonize incoming IoT sensor data into unified format",
                ApiKey = "iot-sensor-demo-key-12345678",
                IsActive = true,
                DesiredOutputType = "JSON",
                TeamId = iotTeam?.Id,
                EntityName = "sensor_readings",
                ContextPrompt = @"You are an IoT data harmonizer. Transform incoming sensor data into a standardized format.
Extract:
- Device ID and type
- All sensor readings with units
- Timestamps in ISO 8601 format
- Status and health indicators
- Any alerts or warnings",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    device = new { id = "", type = "", location = "" },
                    timestamp = "",
                    sensors = new[] { new { name = "", value = 0.0, unit = "" } },
                    status = new { operational = true, health = 100 },
                    alerts = new[] { new { level = "", message = "" } }
                }, new JsonSerializerOptions { WriteIndented = true })
            },
            new Workflow
            {
                Name = "Manufacturing Equipment Monitor",
                Description = "Process and harmonize manufacturing equipment telemetry",
                ApiKey = "mfg-equipment-demo-key-87654",
                IsActive = true,
                DesiredOutputType = "JSON",
                TeamId = mfgTeam?.Id,
                EntityName = "equipment_telemetry",
                ContextPrompt = @"Transform manufacturing equipment data into unified monitoring format.
Focus on:
- Machine identification (ID, type, manufacturer)
- Operational status (running, idle, error, maintenance)
- Performance metrics (OEE, efficiency, throughput)
- Quality indicators
- Maintenance information",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    machine = new { id = "", type = "", manufacturer = "", model = "" },
                    status = new { operational = "", mode = "", health = 0 },
                    performance = new { efficiency = 0.0, oee = 0.0, throughput = 0.0 },
                    production = new { completed = 0, target = 0 },
                    maintenance = new { hoursUntilService = 0.0, lastService = "" }
                }, new JsonSerializerOptions { WriteIndented = true })
            },
            new Workflow
            {
                Name = "Quality Inspection Reporter",
                Description = "Convert quality inspection data to standard report format",
                ApiKey = "quality-inspect-demo-key-2468",
                IsActive = true,
                DesiredOutputType = "JSON",
                TeamId = qualityTeam?.Id,
                EntityName = "inspection_reports",
                ContextPrompt = @"Harmonize quality inspection data into a standardized report format.
Extract:
- Inspection ID and timestamp
- Part/product information
- Measurements and tolerances
- Pass/fail status
- Defect details if any",
                DesiredOutput = JsonSerializer.Serialize(new
                {
                    inspection = new { id = "", timestamp = "", inspector = "" },
                    part = new { partNumber = "", serialNumber = "", batch = "" },
                    result = new { status = "", overallScore = 0.0 },
                    measurements = new[] { new { parameter = "", value = 0.0, unit = "", inSpec = true } },
                    defects = new[] { new { type = "", severity = "", location = "" } }
                }, new JsonSerializerOptions { WriteIndented = true })
            }
        };

        await context.Workflows.AddRangeAsync(workflows);
        await context.SaveChangesAsync();
    }

    private static List<HarmonizeRequest> GenerateSampleRequests()
    {
        var requests = new List<HarmonizeRequest>();
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1. CNC Milling Machine - Production Data
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "CNC-ML-3001",
                machineType = "CNC Milling Machine",
                manufacturer = "DMG MORI",
                model = "NHX 5000",
                location = new { factory = "Plant-A", floor = 2, cell = "Manufacturing Cell 3" },
                timestamp = "2024-12-02T08:15:30Z",
                operationalData = new
                {
                    status = "RUNNING",
                    currentProgram = "PART-5521-REV3",
                    cycleTime = 145.5,
                    partsProduced = 847,
                    targetProduction = 1200,
                    efficiency = 94.2,
                    uptime = 22.5
                },
                sensors = new
                {
                    spindleSpeed = 8500,
                    spindleLoad = 75.2,
                    spindleTemp = 68.5,
                    vibration = 0.23,
                    coolantTemp = 22.5,
                    coolantFlow = 45.5,
                    hydraulicPressure = 52.5
                },
                currentJob = new
                {
                    workOrderId = "WO-2024-5521",
                    partNumber = "ENG-BLOCK-X500",
                    material = "Aluminum 6061",
                    quantity = 50,
                    completed = 23
                }
            }),
            Source = "SCADA_System_A",
            CreatedAt = baseDate.AddDays(1)
        });

        // 2. Industrial Robot Arm - Welding Station
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                robotId = "ROBO-WLD-7722",
                robotType = "6-Axis Industrial Robot",
                manufacturer = "KUKA",
                model = "KR 210 R3100",
                station = new { id = "WELD-STN-05", area = "Assembly Line 2", zone = "Chassis Welding" },
                timestamp = "2024-12-02T09:22:15Z",
                status = new
                {
                    operational = "ACTIVE",
                    mode = "AUTO",
                    program = "WELD-CHASSIS-V2.1",
                    cyclesCompleted = 234,
                    cycleTime = 87.3,
                    targetCycleTime = 85.0
                },
                weldingParams = new
                {
                    current = 185,
                    voltage = 24.5,
                    wireSpeed = 8.5,
                    gasFlow = 15.2,
                    arcStability = 98.5
                },
                robotHealth = new
                {
                    joint1Temp = 45.2,
                    joint2Temp = 47.8,
                    joint3Temp = 44.1,
                    joint4Temp = 42.3,
                    joint5Temp = 43.7,
                    joint6Temp = 41.9,
                    overallHealth = 96.5
                },
                qualityMetrics = new
                {
                    weldQuality = 99.2,
                    penetrationDepth = 4.8,
                    defectCount = 2,
                    reworkRate = 0.8
                }
            }),
            Source = "RobotController_KUKA",
            CreatedAt = baseDate.AddDays(2)
        });

        // 3. Injection Molding Machine
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "MOLD-INJ-4501",
                machineType = "Injection Molding Machine",
                manufacturer = "ENGEL",
                model = "e-victory 440",
                location = new { plant = "Plastics Facility", line = "Production Line 3", position = "Station 12" },
                timestamp = "2024-12-02T10:45:00Z",
                processData = new
                {
                    status = "PRODUCTION",
                    moldId = "MOLD-HOUSING-X12",
                    materialType = "ABS Polymer",
                    shotCount = 8234,
                    cavities = 4,
                    cycleTime = 42.5,
                    targetCycleTime = 40.0
                },
                injectionParams = new
                {
                    injectionPressure = 1850,
                    injectionSpeed = 95,
                    holdingPressure = 850,
                    holdingTime = 8.5,
                    coolingTime = 22.0,
                    barrelTemp1 = 225,
                    barrelTemp2 = 230,
                    barrelTemp3 = 235,
                    moldTemp = 65
                },
                performance = new
                {
                    efficiency = 91.2,
                    rejectRate = 1.8,
                    downtimeMinutes = 45,
                    oee = 88.5
                }
            }),
            Source = "MES_PlasticsPlant",
            CreatedAt = baseDate.AddDays(3)
        });

        // 4. Automated Guided Vehicle (AGV)
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                vehicleId = "AGV-TR-8821",
                vehicleType = "Automated Guided Vehicle",
                manufacturer = "Dematic",
                model = "AGV-500",
                timestamp = "2024-12-02T11:30:22Z",
                location = new
                {
                    currentPosition = new { x = 125.5, y = 78.3, zone = "Warehouse-B" },
                    heading = 285,
                    altitude = 0
                },
                mission = new
                {
                    taskId = "TASK-TR-5521",
                    missionType = "MATERIAL_TRANSPORT",
                    status = "IN_PROGRESS",
                    pickup = new { station = "WH-RACK-A45", item = "PALLET-8821" },
                    delivery = new { station = "ASSY-LINE-2", eta = "2024-12-02T11:42:00Z" },
                    progress = 65
                },
                vehicleHealth = new
                {
                    batteryLevel = 78,
                    batteryVoltage = 48.2,
                    batteryCycles = 234,
                    motorTemp = 52.3,
                    tireCondition = 95,
                    sensorStatus = "OPTIMAL"
                },
                navigation = new
                {
                    speed = 1.2,
                    obstaclesDetected = 0,
                    pathDeviation = 0.05,
                    navigationMode = "AUTONOMOUS"
                }
            }),
            Source = "FleetManagementSystem",
            CreatedAt = baseDate.AddDays(4)
        });

        // 5. Industrial 3D Printer - Additive Manufacturing
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                printerId = "3DP-AM-2201",
                printerType = "Metal 3D Printer",
                manufacturer = "EOS",
                model = "M 290",
                location = new { facility = "Advanced Manufacturing Lab", room = "AM-103" },
                timestamp = "2024-12-02T12:15:45Z",
                buildJob = new
                {
                    jobId = "BUILD-5521-X",
                    partName = "Turbine Blade Prototype",
                    material = "Inconel 718",
                    layerHeight = 0.04,
                    currentLayer = 2847,
                    totalLayers = 4520,
                    progress = 63.0,
                    estimatedCompletion = "2024-12-02T18:30:00Z"
                },
                processParams = new
                {
                    laserPower = 285,
                    scanSpeed = 850,
                    buildPlatformTemp = 80,
                    chamberTemp = 40,
                    oxygenLevel = 0.08,
                    argonFlow = 5.2
                },
                systemHealth = new
                {
                    laserHours = 2847,
                    powderUsage = 4.5,
                    powderRemaining = 18.2,
                    filterStatus = 82,
                    maintenanceDue = "2024-12-15"
                }
            }),
            Source = "AdditiveManufacturing_MES",
            CreatedAt = baseDate.AddDays(5)
        });

        // 6. Assembly Line Conveyor System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                systemId = "CONV-SYS-0301",
                systemType = "Smart Conveyor System",
                manufacturer = "Siemens",
                model = "SIMATIC",
                location = new { plant = "Final Assembly", line = "Line-A", segments = 12 },
                timestamp = "2024-12-02T13:22:08Z",
                operationalStatus = new
                {
                    lineStatus = "RUNNING",
                    speed = 0.85,
                    targetSpeed = 0.90,
                    productCount = 1234,
                    shift = "Day Shift",
                    uptime = 94.5
                },
                conveyorSegments = new[]
                {
                    new { segmentId = "SEG-01", speed = 0.85, load = 78, motorTemp = 58.2, status = "OK" },
                    new { segmentId = "SEG-02", speed = 0.85, load = 82, motorTemp = 59.8, status = "OK" },
                    new { segmentId = "SEG-03", speed = 0.85, load = 75, motorTemp = 57.5, status = "OK" }
                },
                productTracking = new
                {
                    productsInSystem = 45,
                    averageTransitTime = 285,
                    bottleneckStation = "STATION-07",
                    throughput = 342
                },
                alerts = new[]
                {
                    new { id = "ALT-001", severity = "MEDIUM", message = "Segment 07 running below optimal speed" }
                }
            }),
            Source = "ConveyorPLC_Siemens",
            CreatedAt = baseDate.AddDays(6)
        });

        // 7. Industrial Laser Cutter
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "LASER-CUT-6601",
                machineType = "Fiber Laser Cutting System",
                manufacturer = "Trumpf",
                model = "TruLaser 5030",
                location = new { facility = "Sheet Metal Shop", bay = "Bay-3" },
                timestamp = "2024-12-02T14:05:33Z",
                operationalData = new
                {
                    status = "CUTTING",
                    programName = "BRACKET-PATTERN-REV5",
                    materialType = "Stainless Steel 304",
                    materialThickness = 3.0,
                    sheetSize = "2500x1250",
                    partsPerSheet = 24,
                    sheetsProcessed = 67
                },
                laserSystem = new
                {
                    laserPower = 4000,
                    focusPosition = 0.5,
                    assistGas = "Nitrogen",
                    gasPressure = 12.5,
                    nozzleSize = 2.0,
                    beamQuality = 98.5
                },
                performance = new
                {
                    cuttingSpeed = 8.5,
                    edgeQuality = 97.8,
                    piercingTime = 1.2,
                    cycleTime = 6.8,
                    efficiency = 92.3
                },
                maintenance = new
                {
                    laserHours = 8234,
                    nozzleCondition = 85,
                    lensCondition = 92,
                    nextPM = "2024-12-10"
                }
            }),
            Source = "LaserCutter_Controller",
            CreatedAt = baseDate.AddDays(7)
        });

        // 8. Automated Quality Inspection System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                systemId = "QIS-VIS-4401",
                systemType = "Vision Inspection System",
                manufacturer = "Cognex",
                model = "In-Sight 9000",
                location = new { line = "Assembly Line 1", station = "Quality Gate 3" },
                timestamp = "2024-12-02T15:18:42Z",
                inspectionData = new
                {
                    status = "ACTIVE",
                    recipe = "COMPONENT-INSPECT-REV8",
                    partsInspected = 3421,
                    partsAccepted = 3387,
                    partsRejected = 34,
                    passRate = 99.0
                },
                defectAnalysis = new
                {
                    dimensionalDefects = 12,
                    surfaceDefects = 18,
                    missingComponents = 3,
                    orientationErrors = 1,
                    totalDefects = 34
                },
                systemPerformance = new
                {
                    inspectionTime = 0.85,
                    imageQuality = 98,
                    lightingStatus = "OPTIMAL",
                    cameraTemp = 42.5,
                    processingLoad = 67
                },
                calibration = new
                {
                    lastCalibration = "2024-11-28T08:00:00Z",
                    nextCalibration = "2024-12-28T08:00:00Z",
                    accuracy = 99.7
                }
            }),
            Source = "QualityInspectionMES",
            CreatedAt = baseDate.AddDays(8)
        });

        // 9. Industrial Furnace - Heat Treatment
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                furnaceId = "FURN-HT-8801",
                furnaceType = "Continuous Heat Treatment Furnace",
                manufacturer = "Ipsen",
                model = "Titan H2",
                location = new { facility = "Heat Treatment Center", zone = "Zone-B" },
                timestamp = "2024-12-02T16:30:15Z",
                processData = new
                {
                    status = "PROCESSING",
                    recipe = "STEEL-HARDENING-1050C",
                    batchId = "BATCH-HT-5521",
                    loadWeight = 2500,
                    materialType = "Tool Steel"
                },
                temperatureZones = new
                {
                    preheatZone = 650,
                    heatingZone = 1050,
                    soakZone = 1050,
                    coolingZone = 450,
                    quenchTemp = 60
                },
                atmosphere = new
                {
                    atmosphereType = "Nitrogen",
                    oxygenLevel = 0.02,
                    carbonPotential = 0.85,
                    dewPoint = -25,
                    pressure = 1.2
                },
                performance = new
                {
                    heatingRate = 15.5,
                    temperatureUniformity = 98.5,
                    cycleTime = 285,
                    energyConsumption = 1250
                }
            }),
            Source = "FurnaceController_Ipsen",
            CreatedAt = baseDate.AddDays(9)
        });

        // 10. Packaging Robot Cell
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                cellId = "PKG-CELL-1102",
                cellType = "Robotic Packaging Cell",
                manufacturer = "ABB",
                model = "IRB 660",
                location = new { facility = "Packaging Hall", line = "Packaging Line 2" },
                timestamp = "2024-12-02T17:42:28Z",
                operationalData = new
                {
                    status = "RUNNING",
                    mode = "AUTO",
                    packagingPattern = "CASE-PACK-24",
                    unitsPackaged = 5234,
                    targetRate = 120,
                    actualRate = 118
                },
                robotPerformance = new
                {
                    pickSuccessRate = 99.8,
                    placeAccuracy = 99.5,
                    cycleTime = 0.51,
                    gripperStatus = "OPTIMAL",
                    vacuumPressure = 85
                },
                packagingQuality = new
                {
                    properOrientation = 99.7,
                    packageIntegrity = 100.0,
                    labelAccuracy = 99.9,
                    rejectedPackages = 8
                },
                cellHealth = new
                {
                    robotUptime = 96.5,
                    conveyorSpeed = 1.2,
                    sealerTemp = 185,
                    maintenanceStatus = "OK"
                }
            }),
            Source = "PackagingMES_ABB",
            CreatedAt = baseDate.AddDays(10)
        });

        // 11. Industrial Compressor System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                compressorId = "COMP-AIR-7701",
                compressorType = "Rotary Screw Air Compressor",
                manufacturer = "Atlas Copco",
                model = "GA 110 VSD+",
                location = new { facility = "Utility Building", room = "Compressor Room 2" },
                timestamp = "2024-12-02T18:15:50Z",
                operationalData = new
                {
                    status = "LOADED",
                    runningHours = 15234,
                    pressure = 7.5,
                    targetPressure = 7.5,
                    flowRate = 18.5,
                    powerConsumption = 85.2
                },
                motorData = new
                {
                    motorSpeed = 2850,
                    motorLoad = 78.5,
                    motorTemp = 68.2,
                    bearingTemp = 55.8,
                    vibration = 0.15
                },
                airQuality = new
                {
                    dewPoint = -40,
                    oilContent = 0.01,
                    particleContent = 0.1,
                    filterStatus = 85
                },
                efficiency = new
                {
                    specificPower = 4.8,
                    volumetricEfficiency = 94.5,
                    energyRecovery = 72.0,
                    leakageRate = 8.5
                }
            }),
            Source = "CompressorSCADA",
            CreatedAt = baseDate.AddDays(11)
        });

        // 12. Coordinate Measuring Machine (CMM)
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                cmmId = "CMM-INSP-3301",
                cmmType = "Bridge CMM",
                manufacturer = "Hexagon",
                model = "Global S",
                location = new { facility = "Quality Lab", room = "Metrology Lab 1" },
                timestamp = "2024-12-02T19:05:12Z",
                measurementData = new
                {
                    status = "MEASURING",
                    partId = "PART-COMPLEX-X12",
                    program = "INSPECT-PROG-REV4",
                    measurementPoints = 145,
                    completedPoints = 87,
                    progress = 60.0
                },
                accuracy = new
                {
                    probeType = "Scanning Probe",
                    probeRadius = 2.0,
                    measurementUncertainty = 0.0025,
                    temperatureCompensation = true,
                    envTemp = 20.02
                },
                results = new
                {
                    dimensionsChecked = 52,
                    dimensionsInTolerance = 50,
                    dimensionsOutOfTolerance = 2,
                    maxDeviation = 0.035,
                    cpk = 1.67
                },
                systemStatus = new
                {
                    airBearingPressure = 5.8,
                    temperatureStability = 0.15,
                    calibrationStatus = "VALID",
                    lastCalibration = "2024-11-15"
                }
            }),
            Source = "MetrologySystem_Hexagon",
            CreatedAt = baseDate.AddDays(12)
        });

        // 13. Stamping Press Machine
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                pressId = "PRESS-STMP-5501",
                pressType = "Mechanical Stamping Press",
                manufacturer = "Schuler",
                model = "MSP 1000",
                location = new { facility = "Stamping Plant", line = "Press Line 3" },
                timestamp = "2024-12-02T20:22:35Z",
                operationalData = new
                {
                    status = "RUNNING",
                    dieSet = "DIE-PANEL-OUTER-LEFT",
                    strokesPerMinute = 15,
                    strokeCount = 23456,
                    targetProduction = 25000,
                    efficiency = 93.8
                },
                pressForce = new
                {
                    tonnage = 850,
                    bottomDeadCenter = 998,
                    slidePosition = 125.5,
                    cushionPressure = 125
                },
                lubrication = new
                {
                    lubricantLevel = 78,
                    lubricantPressure = 6.5,
                    lubricantTemp = 45.2,
                    flowRate = 2.5
                },
                quality = new
                {
                    partsProduced = 23456,
                    partsAccepted = 23234,
                    scrapRate = 0.95,
                    toolWear = 15.2
                }
            }),
            Source = "PressController_Schuler",
            CreatedAt = baseDate.AddDays(13)
        });

        // 14. Automated Storage and Retrieval System (ASRS)
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                systemId = "ASRS-WH-9901",
                systemType = "Automated Storage Retrieval System",
                manufacturer = "Daifuku",
                model = "Maxi-Load",
                location = new { warehouse = "Central Distribution", aisle = "Aisle-5" },
                timestamp = "2024-12-02T21:10:45Z",
                operationalData = new
                {
                    status = "ACTIVE",
                    currentTask = "RETRIEVAL",
                    tasksCompleted = 567,
                    tasksQueued = 12,
                    systemUtilization = 78.5
                },
                crane = new
                {
                    position = new { x = 145.5, y = 23.2, z = 8.5 },
                    speed = 2.8,
                    loadStatus = "LOADED",
                    loadWeight = 850
                },
                inventory = new
                {
                    totalLocations = 5000,
                    occupiedLocations = 4234,
                    occupancyRate = 84.7,
                    availableLocations = 766
                },
                performance = new
                {
                    averageCycleTime = 85.5,
                    retrievalAccuracy = 99.9,
                    storageAccuracy = 99.9,
                    systemUptime = 97.2
                }
            }),
            Source = "WarehouseManagementSystem",
            CreatedAt = baseDate.AddDays(14)
        });

        // 15. Paint Booth System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                boothId = "PAINT-BOOTH-8801",
                boothType = "Automated Paint Booth",
                manufacturer = "Durr",
                model = "EcoRP E043i",
                location = new { facility = "Paint Shop", zone = "Coating Zone 2" },
                timestamp = "2024-12-02T22:35:18Z",
                operationalData = new
                {
                    status = "SPRAYING",
                    program = "BASE-COAT-METALLIC-BLUE",
                    unitsProcessed = 234,
                    cycleTime = 95.5,
                    targetCycleTime = 90.0
                },
                robotArms = new[]
                {
                    new { armId = "ARM-01", program = "BASE-PATTERN-A", utilization = 92.5, status = "ACTIVE" },
                    new { armId = "ARM-02", program = "BASE-PATTERN-B", utilization = 89.2, status = "ACTIVE" }
                },
                paintSystem = new
                {
                    paintType = "2K Polyurethane",
                    colorCode = "BLUE-METALLIC-501",
                    viscosity = 45,
                    pressure = 2.5,
                    flowRate = 350,
                    atomizingPressure = 1.8
                },
                environmental = new
                {
                    boothTemp = 23.5,
                    humidity = 55,
                    airflow = 25000,
                    overspray = 15.2,
                    filterEfficiency = 98.5
                }
            }),
            Source = "PaintShopMES",
            CreatedAt = baseDate.AddDays(15)
        });

        // 16. Extrusion Line
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                lineId = "EXTR-LINE-6601",
                lineType = "Plastic Extrusion Line",
                manufacturer = "Reifenhauser",
                model = "EVO Ultra",
                location = new { facility = "Extrusion Plant", line = "Line-2" },
                timestamp = "2024-12-02T23:15:42Z",
                operationalData = new
                {
                    status = "PRODUCING",
                    productType = "HDPE Pipe 110mm",
                    lineSpeed = 3.5,
                    targetSpeed = 3.8,
                    output = 245,
                    efficiency = 92.1
                },
                extruderData = new
                {
                    extruderLoad = 85.2,
                    screwSpeed = 45,
                    meltTemp = 220,
                    meltPressure = 185,
                    zone1Temp = 180,
                    zone2Temp = 200,
                    zone3Temp = 215,
                    zone4Temp = 220
                },
                downstream = new
                {
                    dieTemp = 218,
                    vacuumTankPressure = -0.45,
                    coolingTemp = 15,
                    pullerSpeed = 3.5,
                    cutterSpeed = 35
                },
                quality = new
                {
                    wallThickness = 6.8,
                    targetThickness = 6.8,
                    ovalityPercent = 0.5,
                    surfaceQuality = 98.5
                }
            }),
            Source = "ExtrusionControlSystem",
            CreatedAt = baseDate.AddDays(16)
        });

        // 17. Industrial Chiller System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                chillerId = "CHILL-SYS-4401",
                chillerType = "Industrial Water Chiller",
                manufacturer = "Carrier",
                model = "AquaEdge 19DV",
                location = new { facility = "Utility Building", room = "Chiller Room 1" },
                timestamp = "2024-12-03T00:45:22Z",
                operationalData = new
                {
                    status = "RUNNING",
                    mode = "COOLING",
                    chilledWaterSupply = 7.2,
                    chilledWaterReturn = 12.5,
                    coolingCapacity = 850,
                    loadPercent = 72.5
                },
                compressor = new
                {
                    compressor1Status = "RUNNING",
                    compressor2Status = "STANDBY",
                    dischargePressure = 12.5,
                    suctionPressure = 4.8,
                    compressorAmps = 245,
                    oilPressure = 3.5
                },
                refrigerant = new
                {
                    refrigerantType = "R-134a",
                    refrigerantCharge = 98.5,
                    subcooling = 5.2,
                    superheat = 8.5
                },
                efficiency = new
                {
                    cop = 5.8,
                    kWPerTon = 0.61,
                    energyConsumption = 125.5,
                    waterFlowRate = 145
                }
            }),
            Source = "ChillerBMS",
            CreatedAt = baseDate.AddDays(17)
        });

        // 18. Automated Welding Cell
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                cellId = "WELD-CELL-7702",
                cellType = "Automated Welding Cell",
                manufacturer = "Fanuc",
                model = "ARC Mate 120iC",
                location = new { facility = "Fabrication Shop", cell = "Weld Cell 5" },
                timestamp = "2024-12-03T01:30:15Z",
                operationalData = new
                {
                    status = "WELDING",
                    program = "FRAME-WELD-SEQ-12",
                    partsCompleted = 156,
                    cycleTime = 325,
                    targetCycleTime = 310
                },
                weldingProcess = new
                {
                    processType = "GMAW",
                    weldingCurrent = 245,
                    weldingVoltage = 28.5,
                    wireSpeed = 9.5,
                    travelSpeed = 8.5,
                    shieldingGas = "Ar-CO2 Mix"
                },
                torchData = new
                {
                    torchAngle = 15,
                    stickout = 15,
                    gasFlow = 18.5,
                    wireFeed = "OPTIMAL",
                    nozzleCondition = 92
                },
                quality = new
                {
                    weldLength = 2456,
                    weldQualityScore = 98.5,
                    defectsDetected = 3,
                    reworkRequired = 2
                }
            }),
            Source = "WeldingCellController",
            CreatedAt = baseDate.AddDays(18)
        });

        // 19. Turbine Generator Monitoring
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                generatorId = "TURB-GEN-9901",
                generatorType = "Gas Turbine Generator",
                manufacturer = "GE",
                model = "LM6000",
                location = new { facility = "Power Generation Plant", unit = "Unit-1" },
                timestamp = "2024-12-03T02:15:48Z",
                operationalData = new
                {
                    status = "GENERATING",
                    loadMW = 42.5,
                    targetLoadMW = 45.0,
                    frequency = 60.0,
                    voltage = 13800,
                    powerFactor = 0.95
                },
                turbineData = new
                {
                    turbineSpeed = 3600,
                    exhaustTemp = 485,
                    compressorInletTemp = 25,
                    compressorDischargeTemp = 385,
                    fuelFlowRate = 2.5
                },
                vibration = new
                {
                    bearingVibration1 = 0.15,
                    bearingVibration2 = 0.18,
                    bearingVibration3 = 0.14,
                    bearingTemp1 = 75.2,
                    bearingTemp2 = 78.5,
                    bearingTemp3 = 76.8
                },
                performance = new
                {
                    efficiency = 38.5,
                    heatRate = 9850,
                    runningHours = 45234,
                    startsSinceOverhaul = 234
                }
            }),
            Source = "PowerPlantDCS",
            CreatedAt = baseDate.AddDays(19)
        });

        // 20. Industrial Mixer System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                mixerId = "MIX-BLEND-5501",
                mixerType = "High Shear Mixer",
                manufacturer = "Silverson",
                model = "FX100",
                location = new { facility = "Chemical Processing", room = "Batch Room 3" },
                timestamp = "2024-12-03T03:05:30Z",
                batchData = new
                {
                    status = "MIXING",
                    batchId = "BATCH-CHEM-5521",
                    recipe = "FORMULA-X-REV8",
                    batchSize = 5000,
                    mixingTime = 45,
                    targetTime = 60
                },
                processParams = new
                {
                    motorSpeed = 3500,
                    motorLoad = 82.5,
                    torque = 125,
                    temperature = 55.5,
                    viscosity = 1250,
                    density = 1.18
                },
                ingredients = new[]
                {
                    new { ingredientId = "ING-001", name = "Base Polymer", added = true, quantity = 2500 },
                    new { ingredientId = "ING-015", name = "Catalyst A", added = true, quantity = 125 },
                    new { ingredientId = "ING-023", name = "Additive B", added = true, quantity = 75 }
                },
                quality = new
                {
                    homogeneity = 98.5,
                    particleSize = 25.5,
                    colorMatch = 99.2,
                    phLevel = 7.2
                }
            }),
            Source = "BatchManagementSystem",
            CreatedAt = baseDate.AddDays(20)
        });

        // 21. Wire EDM Machine
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "EDM-WIRE-3301",
                machineType = "Wire EDM",
                manufacturer = "Mitsubishi",
                model = "MV2400S",
                location = new { facility = "Precision Machining", area = "EDM Department" },
                timestamp = "2024-12-03T04:22:15Z",
                operationalData = new
                {
                    status = "CUTTING",
                    program = "DIE-CAVITY-COMPLEX-REV2",
                    workpiece = "Tool Steel H13",
                    thickness = 150,
                    wireType = "Brass 0.25mm",
                    roughPass = false,
                    finishPass = true
                },
                processParams = new
                {
                    wireSpeed = 12,
                    wireTension = 1200,
                    dischargeCurrent = 8.5,
                    pulseOnTime = 0.8,
                    pulseOffTime = 2.5,
                    dielectricTemp = 22.5
                },
                accuracy = new
                {
                    surfaceFinish = 0.4,
                    dimensionalAccuracy = 0.003,
                    cornerRadius = 0.12,
                    straightness = 0.005
                },
                consumption = new
                {
                    wireUsed = 2456,
                    wireRemaining = 15234,
                    dielectricFlow = 12.5,
                    filterStatus = 88
                }
            }),
            Source = "EDM_Controller",
            CreatedAt = baseDate.AddDays(21)
        });

        // 22. Hydraulic Press Brake
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                pressId = "BRAKE-HYDR-8801",
                pressType = "CNC Hydraulic Press Brake",
                manufacturer = "Amada",
                model = "HFE 220",
                location = new { facility = "Sheet Metal Fabrication", bay = "Bending Bay 2" },
                timestamp = "2024-12-03T05:10:45Z",
                operationalData = new
                {
                    status = "BENDING",
                    program = "BRACKET-BEND-SEQ-5",
                    material = "Cold Rolled Steel",
                    thickness = 3.0,
                    bendCount = 3456,
                    cycleTime = 8.5
                },
                bendingParams = new
                {
                    bendAngle = 90.0,
                    bendLength = 1200,
                    tonnage = 180,
                    backGaugePosition = 125.5,
                    crowningValue = 0.8
                },
                hydraulicSystem = new
                {
                    systemPressure = 185,
                    oilTemp = 45.5,
                    oilLevel = 92,
                    pumpLoad = 78.5
                },
                accuracy = new
                {
                    angleAccuracy = 0.25,
                    positionAccuracy = 0.05,
                    repeatability = 0.02,
                    qualityScore = 98.8
                }
            }),
            Source = "PressBrake_CNC",
            CreatedAt = baseDate.AddDays(22)
        });

        // 23. Powder Coating Line
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                lineId = "COAT-LINE-4401",
                lineType = "Powder Coating Line",
                manufacturer = "Nordson",
                model = "Versa-Spray II",
                location = new { facility = "Finishing Department", line = "Coating Line 1" },
                timestamp = "2024-12-03T06:35:22Z",
                operationalData = new
                {
                    status = "COATING",
                    partsCoated = 2345,
                    lineSpeed = 2.5,
                    targetSpeed = 2.8,
                    efficiency = 89.3
                },
                pretreatment = new
                {
                    cleanerTemp = 65,
                    rinseTemp = 25,
                    phosphateConcentration = 12.5,
                    phLevel = 9.8
                },
                powderApplication = new
                {
                    powderType = "Epoxy Polyester",
                    colorCode = "RAL-9005",
                    voltage = 85,
                    currentMicroAmps = 45,
                    sprayRate = 225,
                    recoveryRate = 95.5
                },
                curing = new
                {
                    ovenTemp = 195,
                    dwellTime = 15,
                    airflow = 12000,
                    filmThickness = 75
                }
            }),
            Source = "CoatingLinePLC",
            CreatedAt = baseDate.AddDays(23)
        });

        // 24. Vertical Machining Center
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "VMC-MACH-7701",
                machineType = "Vertical Machining Center",
                manufacturer = "Haas",
                model = "VF-4SS",
                location = new { facility = "Machine Shop", cell = "Machining Cell 7" },
                timestamp = "2024-12-03T07:50:18Z",
                operationalData = new
                {
                    status = "MACHINING",
                    program = "OP20-HOUSING-FINISH",
                    workpiece = "Aluminum 7075",
                    toolNumber = 12,
                    partsCompleted = 67,
                    cycleTime = 18.5
                },
                machiningParams = new
                {
                    spindleSpeed = 12000,
                    feedRate = 3500,
                    depthOfCut = 2.5,
                    coolantFlow = 45,
                    coolantPressure = 15.5
                },
                toolManagement = new
                {
                    totalTools = 40,
                    toolsInUse = 12,
                    toolLife = 78,
                    nextToolChange = "TOOL-15",
                    toolChanges = 234
                },
                maintenance = new
                {
                    powerOnHours = 12456,
                    cuttingHours = 8234,
                    rapidTraverseDistance = 125678,
                    nextService = "2024-12-15"
                }
            }),
            Source = "VMC_Controller_Haas",
            CreatedAt = baseDate.AddDays(24)
        });

        // 25. Screw Compressor - Plant Air
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                compressorId = "COMP-SCREW-9901",
                compressorType = "Oil-Free Screw Compressor",
                manufacturer = "Ingersoll Rand",
                model = "Nirvana N90",
                location = new { facility = "Utility Building", room = "Air Compressor Room" },
                timestamp = "2024-12-03T08:25:45Z",
                operationalData = new
                {
                    status = "RUNNING",
                    mode = "LOAD",
                    dischargePressure = 7.8,
                    flowRate = 90,
                    powerDraw = 75.5,
                    runningHours = 23456
                },
                stages = new
                {
                    stage1Temp = 85,
                    stage2Temp = 95,
                    intercoolerTemp = 42,
                    aftercoolerTemp = 35
                },
                airQuality = new
                {
                    dewPoint = -40,
                    particleCount = 0.01,
                    waterVaporContent = 0.002,
                    filterPressureDrop = 0.15
                },
                efficiency = new
                {
                    specificPower = 4.2,
                    isentropicEfficiency = 88.5,
                    capacityUtilization = 82.5,
                    energyRecovered = 68
                }
            }),
            Source = "PlantAirSystem",
            CreatedAt = baseDate.AddDays(25)
        });

        // 26. Plasma Cutting Table
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                tableId = "PLASMA-CUT-5501",
                tableType = "CNC Plasma Cutting Table",
                manufacturer = "Hypertherm",
                model = "XPR300",
                location = new { facility = "Fabrication Shop", area = "Cutting Area 1" },
                timestamp = "2024-12-03T09:15:30Z",
                operationalData = new
                {
                    status = "CUTTING",
                    program = "PLATE-PATTERN-NEST-45",
                    material = "Mild Steel A36",
                    thickness = 12.0,
                    sheetSize = "3000x1500",
                    partsPerSheet = 18
                },
                cuttingParams = new
                {
                    current = 200,
                    voltage = 155,
                    cuttingSpeed = 2500,
                    pierceHeight = 3.8,
                    cuttingHeight = 3.0,
                    arcVoltage = 148
                },
                consumables = new
                {
                    electrodeLife = 85,
                    nozzleLife = 78,
                    shieldLife = 92,
                    pierces = 1234,
                    arcOnTime = 456
                },
                quality = new
                {
                    edgeQuality = 3,
                    angularity = 2,
                    kerf = 1.8,
                    drossAmount = "Minimal"
                }
            }),
            Source = "PlasmaCutter_Controller",
            CreatedAt = baseDate.AddDays(26)
        });

        // 27. Gear Hobbing Machine
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "HOBB-GEAR-6601",
                machineType = "CNC Gear Hobbing Machine",
                manufacturer = "Liebherr",
                model = "LC 280",
                location = new { facility = "Gear Manufacturing", department = "Hobbing Department" },
                timestamp = "2024-12-03T10:42:15Z",
                operationalData = new
                {
                    status = "HOBBING",
                    gearType = "Spur Gear",
                    module = 3.0,
                    teethCount = 48,
                    material = "Case Hardened Steel",
                    gearsProduced = 145
                },
                cuttingParams = new
                {
                    hobbingSpeed = 185,
                    feedRate = 2.8,
                    depthOfCut = 9.5,
                    numberOfCuts = 1,
                    coolantFlow = 35
                },
                toolData = new
                {
                    hobType = "Single Start Hob",
                    hobDiameter = 100,
                    hobMaterial = "HSS-Cobalt",
                    hobLife = 72,
                    regsharpenings = 5
                },
                quality = new
                {
                    profileAccuracy = "DIN 6",
                    pitchError = 0.008,
                    toothThickness = 4.712,
                    surfaceFinish = 1.6
                }
            }),
            Source = "HobbingMachine_CNC",
            CreatedAt = baseDate.AddDays(27)
        });

        // 28. Industrial Dryer System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                dryerId = "DRYER-IND-8801",
                dryerType = "Rotary Dryer",
                manufacturer = "Buhler",
                model = "DRRD 120",
                location = new { facility = "Material Processing", section = "Drying Section 2" },
                timestamp = "2024-12-03T11:30:48Z",
                operationalData = new
                {
                    status = "DRYING",
                    material = "Plastic Pellets PET",
                    batchId = "BATCH-DRY-5521",
                    throughput = 1200,
                    retentionTime = 25
                },
                temperatureZones = new
                {
                    inletTemp = 180,
                    zone1Temp = 175,
                    zone2Temp = 170,
                    zone3Temp = 165,
                    outletTemp = 160,
                    ambientTemp = 24
                },
                airflow = new
                {
                    inletAirflow = 8500,
                    exhaustAirflow = 8200,
                    pressure = 125,
                    humidity = 12
                },
                performance = new
                {
                    moistureContentIn = 8.5,
                    moistureContentOut = 0.02,
                    dryingEfficiency = 99.8,
                    energyConsumption = 145
                }
            }),
            Source = "DryerControlSystem",
            CreatedAt = baseDate.AddDays(28)
        });

        // 29. Tube Bending Machine
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                machineId = "BEND-TUBE-7701",
                machineType = "CNC Tube Bending Machine",
                manufacturer = "BLM Group",
                model = "E-TURN 40",
                location = new { facility = "Tube Fabrication", area = "Bending Cell 3" },
                timestamp = "2024-12-03T12:15:22Z",
                operationalData = new
                {
                    status = "BENDING",
                    program = "EXHAUST-TUBE-PATTERN-8",
                    tubeMaterial = "Stainless Steel 304",
                    tubeDiameter = 38.1,
                    wallThickness = 1.5,
                    partsCompleted = 234
                },
                bendingParams = new
                {
                    bendAngle = 127.5,
                    centerlineRadius = 76,
                    rotationAngle = 45,
                    bendSpeed = 15,
                    boostPressure = 125
                },
                tooling = new
                {
                    mandrelType = "Ball Mandrel",
                    wiper = "Standard",
                    clampDie = "Split",
                    pressureDie = "Roller"
                },
                quality = new
                {
                    wallThinning = 8.5,
                    ovality = 2.1,
                    springback = 0.8,
                    surfaceQuality = 98.5
                }
            }),
            Source = "TubeBender_Controller",
            CreatedAt = baseDate.AddDays(29)
        });

        // 30. Ultrasonic Cleaning System
        requests.Add(new HarmonizeRequest
        {
            Payload = JsonSerializer.Serialize(new
            {
                systemId = "CLEAN-ULTRA-4401",
                systemType = "Industrial Ultrasonic Cleaner",
                manufacturer = "Crest Ultrasonics",
                model = "ProPlus CP2800HT",
                location = new { facility = "Parts Cleaning", station = "Cleaning Station 5" },
                timestamp = "2024-12-03T13:45:15Z",
                operationalData = new
                {
                    status = "CLEANING",
                    recipe = "PRECISION-PARTS-DEGREASE",
                    basketId = "BASKET-045",
                    cycleTime = 15,
                    loadWeight = 45
                },
                processParams = new
                {
                    frequency = 40,
                    power = 1200,
                    temperature = 65,
                    cleaningSolution = "Alkaline Detergent 5%",
                    solutionLevel = 95
                },
                tankConditions = new
                {
                    tankVolume = 280,
                    solutionConcentration = 5.2,
                    phLevel = 11.5,
                    conductivity = 2500,
                    contaminationLevel = 12
                },
                performance = new
                {
                    cleaningEfficiency = 99.5,
                    partsProcessed = 567,
                    solutionChangeDue = "2024-12-05",
                    filterStatus = 88
                }
            }),
            Source = "CleaningSystemPLC",
            CreatedAt = baseDate.AddDays(30)
        });

        return requests;
    }
    
    private static async Task SeedDynamicTablesAndData(HarmonizerDbContext context)
    {
        // Check if we've already seeded dynamic tables
        var checkSql = "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE 'acme_%'";
        var existingTables = await context.Database.SqlQueryRaw<string>(checkSql).ToListAsync();
        if (existingTables.Any())
        {
            return; // Tables already created
        }

        // Get workflows with team assignments
        var workflows = await context.Workflows
            .Include(w => w.Team)
                .ThenInclude(t => t!.Namespace)
                    .ThenInclude(n => n.Tenant)
            .Where(w => w.TeamId != null && w.EntityName != null)
            .ToListAsync();

        foreach (var workflow in workflows)
        {
            try
            {
                // Generate table name
                var tableName = GenerateTableName(workflow);
                
                // Create table based on desired output schema
                var createTableSql = GenerateCreateTableSql(tableName, workflow.DesiredOutput);
                await context.Database.ExecuteSqlRawAsync(createTableSql);
                
                // Insert sample data
                var sampleData = GenerateSampleDataForWorkflow(workflow);
                foreach (var data in sampleData)
                {
                    var insertSql = GenerateInsertSql(tableName, data, workflow.Id);
                    await context.Database.ExecuteSqlRawAsync(insertSql);
                }
                
                Console.WriteLine($"Created table and seeded data: {tableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table for workflow {workflow.Name}: {ex.Message}");
            }
        }
    }
    
    private static string GenerateTableName(Workflow workflow)
    {
        var parts = new List<string>();
        
        if (workflow.Team?.Namespace?.Tenant != null)
        {
            parts.Add(SanitizeForTableName(workflow.Team.Namespace.Tenant.Slug));
            parts.Add(SanitizeForTableName(workflow.Team.Namespace.Slug));
            parts.Add(SanitizeForTableName(workflow.Team.Slug));
        }
        
        if (!string.IsNullOrEmpty(workflow.EntityName))
        {
            parts.Add(SanitizeForTableName(workflow.EntityName));
        }
        
        return string.Join("_", parts);
    }
    
    private static string GenerateCreateTableSql(string tableName, string jsonSchema)
    {
        // Simple column extraction - in production, use DynamicTableService
        var columns = new List<string>
        {
            "id INTEGER PRIMARY KEY AUTOINCREMENT",
            "created_at DATETIME DEFAULT CURRENT_TIMESTAMP",
            "workflow_id INTEGER",
            "request_id INTEGER",
            "data TEXT" // Store full JSON for now
        };

        var sql = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (\n  {string.Join(",\n  ", columns)}\n);";
        return sql;
    }
    
    private static string GenerateInsertSql(string tableName, string jsonData, int workflowId)
    {
        var escapedData = jsonData.Replace("'", "''");
        return $"INSERT INTO \"{tableName}\" (workflow_id, data) VALUES ({workflowId}, '{escapedData}');";
    }
    
    private static List<string> GenerateSampleDataForWorkflow(Workflow workflow)
    {
        var samples = new List<string>();
        var now = DateTime.UtcNow;
        
        // IoT Sensor Data - 5 diverse samples
        if (workflow.EntityName == "sensor_readings")
        {
            samples.Add(JsonSerializer.Serialize(new
            {
                device = new { id = "TEMP-001", type = "temperature_sensor", location = "Building-A Floor-2" },
                timestamp = now.AddMinutes(-30).ToString("o"),
                sensors = new[]
                {
                    new { name = "temperature", value = 22.5, unit = "celsius" },
                    new { name = "humidity", value = 45.2, unit = "percent" }
                },
                status = new { operational = true, health = 98 },
                alerts = new object[] { }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                device = new { id = "TEMP-002", type = "temperature_sensor", location = "Building-A Floor-3" },
                timestamp = now.AddMinutes(-25).ToString("o"),
                sensors = new[]
                {
                    new { name = "temperature", value = 23.1, unit = "celsius" },
                    new { name = "humidity", value = 48.7, unit = "percent" }
                },
                status = new { operational = true, health = 95 },
                alerts = new[] { new { level = "warning", message = "Humidity above threshold" } }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                device = new { id = "PRESSURE-101", type = "pressure_sensor", location = "Building-B Tank-5" },
                timestamp = now.AddMinutes(-20).ToString("o"),
                sensors = new[]
                {
                    new { name = "pressure", value = 2.5, unit = "bar" },
                    new { name = "temperature", value = 68.3, unit = "celsius" }
                },
                status = new { operational = true, health = 100 },
                alerts = new object[] { }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                device = new { id = "VIBRATION-205", type = "vibration_sensor", location = "Building-C Pump-12" },
                timestamp = now.AddMinutes(-15).ToString("o"),
                sensors = new[]
                {
                    new { name = "vibration_x", value = 0.12, unit = "mm/s" },
                    new { name = "vibration_y", value = 0.15, unit = "mm/s" },
                    new { name = "vibration_z", value = 0.08, unit = "mm/s" }
                },
                status = new { operational = true, health = 92 },
                alerts = new object[] { }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                device = new { id = "ENERGY-301", type = "power_meter", location = "Building-A Main Panel" },
                timestamp = now.AddMinutes(-10).ToString("o"),
                sensors = new[]
                {
                    new { name = "voltage", value = 240.5, unit = "V" },
                    new { name = "current", value = 125.3, unit = "A" },
                    new { name = "power", value = 30.1, unit = "kW" }
                },
                status = new { operational = true, health = 97 },
                alerts = new object[] { }
            }));
        }
        
        // Manufacturing Equipment Telemetry - 5 diverse samples
        else if (workflow.EntityName == "equipment_telemetry")
        {
            samples.Add(JsonSerializer.Serialize(new
            {
                machine = new { id = "CNC-001", type = "CNC_MACHINE", manufacturer = "DMG MORI", model = "NHX 5000" },
                timestamp = now.AddMinutes(-35).ToString("o"),
                status = new { operational = "RUNNING", mode = "AUTO", health = 94 },
                performance = new { efficiency = 92.5, oee = 88.3, throughput = 45.2 },
                production = new { completed = 847, target = 1000, currentCycle = 145.5 },
                maintenance = new { hoursUntilService = 120.5, lastService = "2026-01-15" }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                machine = new { id = "CNC-002", type = "CNC_MACHINE", manufacturer = "HAAS", model = "VF-4SS" },
                timestamp = now.AddMinutes(-28).ToString("o"),
                status = new { operational = "IDLE", mode = "MANUAL", health = 89 },
                performance = new { efficiency = 0.0, oee = 0.0, throughput = 0.0 },
                production = new { completed = 523, target = 800, currentCycle = 0.0 },
                maintenance = new { hoursUntilService = 45.2, lastService = "2026-01-10" }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                machine = new { id = "ROBOT-ARM-15", type = "INDUSTRIAL_ROBOT", manufacturer = "KUKA", model = "KR 120 R2500" },
                timestamp = now.AddMinutes(-22).ToString("o"),
                status = new { operational = "RUNNING", mode = "AUTO", health = 96 },
                performance = new { efficiency = 95.8, oee = 91.2, throughput = 180.5 },
                production = new { completed = 2145, target = 2400, currentCycle = 12.3 },
                maintenance = new { hoursUntilService = 250.0, lastService = "2026-01-05" }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                machine = new { id = "3DPRINT-07", type = "3D_PRINTER", manufacturer = "Stratasys", model = "F900" },
                timestamp = now.AddMinutes(-18).ToString("o"),
                status = new { operational = "RUNNING", mode = "AUTO", health = 88 },
                performance = new { efficiency = 78.5, oee = 72.1, throughput = 0.025 },
                production = new { completed = 12, target = 15, currentCycle = 1854.2 },
                maintenance = new { hoursUntilService = 85.5, lastService = "2026-01-12" }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                machine = new { id = "CONVEYOR-A3", type = "CONVEYOR_SYSTEM", manufacturer = "Dorner", model = "AquaPruf 7600" },
                timestamp = now.AddMinutes(-12).ToString("o"),
                status = new { operational = "RUNNING", mode = "AUTO", health = 100 },
                performance = new { efficiency = 99.2, oee = 97.8, throughput = 3600.0 },
                production = new { completed = 45234, target = 50000, currentCycle = 1.0 },
                maintenance = new { hoursUntilService = 500.0, lastService = "2025-12-20" }
            }));
        }
        
        // Quality Inspection Reports - 5 diverse samples
        else if (workflow.EntityName == "inspection_reports")
        {
            samples.Add(JsonSerializer.Serialize(new
            {
                inspection = new { id = "INS-2026-001", timestamp = now.AddHours(-2).ToString("o"), inspector = "John Smith" },
                part = new { partNumber = "ENG-BLOCK-X500", serialNumber = "SN-001234", batch = "BATCH-456" },
                result = new { status = "PASS", overallScore = 98.5 },
                measurements = new[]
                {
                    new { parameter = "Dimension-A", value = 50.02, unit = "mm", inSpec = true },
                    new { parameter = "Dimension-B", value = 25.01, unit = "mm", inSpec = true },
                    new { parameter = "Surface-Roughness", value = 1.2, unit = "μm", inSpec = true }
                },
                defects = new object[] { }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                inspection = new { id = "INS-2026-002", timestamp = now.AddHours(-1.5).ToString("o"), inspector = "Jane Doe" },
                part = new { partNumber = "ENG-BLOCK-X500", serialNumber = "SN-001235", batch = "BATCH-456" },
                result = new { status = "FAIL", overallScore = 75.0 },
                measurements = new[]
                {
                    new { parameter = "Dimension-A", value = 50.25, unit = "mm", inSpec = false },
                    new { parameter = "Dimension-B", value = 25.00, unit = "mm", inSpec = true },
                    new { parameter = "Surface-Roughness", value = 2.8, unit = "μm", inSpec = false }
                },
                defects = new[]
                {
                    new { type = "Dimensional", severity = "MINOR", location = "Surface-A" }
                }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                inspection = new { id = "INS-2026-003", timestamp = now.AddHours(-1).ToString("o"), inspector = "Mike Johnson" },
                part = new { partNumber = "GEAR-G45", serialNumber = "SN-G45-8821", batch = "BATCH-789" },
                result = new { status = "PASS", overallScore = 99.2 },
                measurements = new[]
                {
                    new { parameter = "Tooth-Width", value = 12.00, unit = "mm", inSpec = true },
                    new { parameter = "Pitch-Diameter", value = 120.01, unit = "mm", inSpec = true },
                    new { parameter = "Runout", value = 0.008, unit = "mm", inSpec = true }
                },
                defects = new object[] { }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                inspection = new { id = "INS-2026-004", timestamp = now.AddMinutes(-45).ToString("o"), inspector = "Sarah Lee" },
                part = new { partNumber = "SHAFT-S200", serialNumber = "SN-S200-4455", batch = "BATCH-321" },
                result = new { status = "CONDITIONAL", overallScore = 85.5 },
                measurements = new[]
                {
                    new { parameter = "Diameter", value = 49.98, unit = "mm", inSpec = true },
                    new { parameter = "Straightness", value = 0.05, unit = "mm", inSpec = true },
                    new { parameter = "Hardness", value = 58.2, unit = "HRC", inSpec = false }
                },
                defects = new[]
                {
                    new { type = "Material", severity = "MINOR", location = "Core Hardness" }
                }
            }));
            
            samples.Add(JsonSerializer.Serialize(new
            {
                inspection = new { id = "INS-2026-005", timestamp = now.AddMinutes(-30).ToString("o"), inspector = "David Chen" },
                part = new { partNumber = "BRACKET-BR12", serialNumber = "SN-BR12-9987", batch = "BATCH-654" },
                result = new { status = "PASS", overallScore = 96.8 },
                measurements = new[]
                {
                    new { parameter = "Hole-Diameter", value = 10.01, unit = "mm", inSpec = true },
                    new { parameter = "Hole-Position", value = 25.00, unit = "mm", inSpec = true },
                    new { parameter = "Flatness", value = 0.02, unit = "mm", inSpec = true }
                },
                defects = new object[] { }
            }));
        }
        
        return samples;
    }
    
    private static string SanitizeForTableName(string input)
    {
        if (string.IsNullOrEmpty(input)) return "unnamed";
        var result = input.ToLowerInvariant();
        result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9]", "_");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"_+", "_");
        return result.Trim('_');
    }
}
