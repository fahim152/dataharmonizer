-- Seed Prompts
INSERT INTO Prompts (Name, ContextPrompt, DesiredOutput, DesiredOutputType, Description, CreatedAt) VALUES
('Industry 4.0 - Unified Equipment Format',
'You are an Industry 4.0 data harmonization engine. Transform the provided machine/equipment data into a standardized Industrial IoT format.

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

OUTPUT: Generate ONLY valid JSON matching the desired schema. Use camelCase consistently.',
'{"harmonizedData":{"equipment":{"id":"","type":"","manufacturer":"","model":"","location":{"facility":"","area":"","zone":"","cell":""}},"timestamp":"","status":{"operational":"","mode":"","health":0,"currentProgram":""},"performance":{"efficiency":0.0,"uptime":0.0,"cycleTime":{"actual":0.0,"target":0.0},"throughput":0.0},"metrics":{"production":{"completed":0,"target":0,"percentComplete":0.0},"quality":{"score":0.0,"defectRate":0.0,"passRate":0.0}},"telemetry":{},"alerts":[{"id":"","severity":"","message":"","timestamp":""}],"maintenance":{"operatingHours":0.0,"nextService":"","healthIndicators":{}},"energy":{"powerConsumption":0.0,"energyUnit":"kW"},"process":{"jobId":"","material":"","parameters":{}},"audit":{"source":"","originalRecordType":"","harmonizedAt":"","schemaVersion":"Industry4.0-v1.0"}}}',
'JSON',
'Transform diverse machine data into standardized Industrial IoT format',
datetime('now'));

-- Seed more prompts with simplified schemas
INSERT INTO Prompts (Name, ContextPrompt, DesiredOutput, DesiredOutputType, Description, CreatedAt) VALUES
('CNC Machine - Production Report',
'Transform this CNC machine data into a production report format. Extract: Machine identification and status, Production metrics (parts produced, efficiency, cycle times), Spindle and tool information, Quality indicators, Maintenance status. Focus on operational KPIs and production performance.',
'{"report":{"machine":{"id":"","type":"CNC","status":""},"production":{"partsProduced":0,"target":0,"efficiency":0.0,"cycleTime":0.0},"spindle":{"speed":0,"load":0.0,"temperature":0.0},"quality":{"defectRate":0.0,"inSpec":true},"maintenance":{"hoursUntilService":0.0,"status":"OK"}}}',
'JSON',
'Extract production metrics from CNC machine data',
datetime('now')),

('Robot - Welding Quality Report',
'Analyze this industrial robot welding data and create a quality report. Extract: Welding parameters (current, voltage, wire speed), Quality metrics (weld quality score, defect count, rework rate), Robot health indicators (joint temperatures, overall health), Cycle performance. Highlight any quality concerns or deviations from standards.',
'{"weldingReport":{"robot":{"id":"","status":"","program":""},"weldingParams":{"current":0,"voltage":0.0,"wireSpeed":0.0,"gasFlow":0.0},"quality":{"weldQuality":0.0,"penetrationDepth":0.0,"defectCount":0,"reworkRate":0.0},"robotHealth":{"overallHealth":0.0,"jointTemps":[]},"performance":{"cyclesCompleted":0,"cycleTime":0.0}}}',
'JSON',
'Extract welding quality metrics from robot data',
datetime('now')),

('AGV - Fleet Status Summary',
'Create an AGV fleet status summary from this vehicle data. Include: Current position and mission status, Battery health and charge level, Navigation performance, Task completion progress. Provide actionable insights for fleet management.',
'{"agvStatus":{"vehicle":{"id":"","type":"AGV"},"position":{"x":0.0,"y":0.0,"zone":"","heading":0},"mission":{"taskId":"","status":"","progress":0,"eta":""},"battery":{"level":0,"voltage":0.0,"estimatedRange":0.0},"navigation":{"speed":0.0,"mode":"","obstaclesDetected":0}}}',
'JSON',
'Summarize AGV mission and health status',
datetime('now')),

('Quality Inspection - Defect Analysis',
'Analyze this quality inspection system data and generate a defect analysis report. Extract: Inspection metrics (parts inspected, pass/fail rates), Defect categorization and counts, Quality trends and patterns, System calibration status. Identify the most common defect types and their frequencies.',
'{"qualityReport":{"inspection":{"system":"","recipe":"","partsInspected":0},"results":{"passRate":0.0,"rejectRate":0.0,"partsRejected":0},"defects":{"dimensional":0,"surface":0,"missing":0,"orientation":0,"total":0},"performance":{"inspectionTime":0.0,"accuracy":0.0},"calibration":{"lastCalibration":"","accuracy":0.0}}}',
'JSON',
'Analyze quality inspection data for defect patterns',
datetime('now'));

