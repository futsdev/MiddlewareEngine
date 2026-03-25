// Test Case Builder - Enhanced with Ordering Controls, Delay & Retry
let draggedElement = null;
let selectedElement = null;
let elementCounter = 0;
let availableFunctions = [];
let currentFunctionFilter = 'All';
let testCaseData = {
    Name: '',
    Description: '',
    Tags: [],
    Priority: 'Medium',
    Status: 'Draft',
    SetupOperations: [],
    Steps: [],
    TeardownOperations: []
};

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    initializeDragAndDrop();
    updateAllCounts();
    loadAvailableFunctions();
    
    // Check if editing existing test case
    const urlParams = new URLSearchParams(window.location.search);
    const testCaseId = urlParams.get('id');
    if (testCaseId) {
        console.log('Loading test case for editing:', testCaseId);
        loadTestCase(testCaseId);
    }
});

async function loadAvailableFunctions() {
    try {
        const response = await fetch('/api/functions');
        if (response.ok) {
            availableFunctions = await response.json();
            console.log('Loaded functions:', availableFunctions.length);
        } else {
            console.error('Failed to load functions:', response.status);
        }
    } catch (error) {
        console.error('Error loading functions:', error);
        showNotification('⚠ Could not load function definitions', 'error');
    }
}

function renderAvailableFunctions() {
    const container = document.getElementById('availableFunctionsList');
    const countEl = document.getElementById('functionsCount');
    
    // If elements don't exist (section was removed), just return
    if (!container || !countEl) {
        return;
    }
    
    if (!availableFunctions || availableFunctions.length === 0) {
        container.innerHTML = '<div style="padding: 0.75rem; text-align: center; color: #888; font-size: 0.8rem;">No functions available</div>';
        countEl.textContent = '(0)';
        return;
    }
    
    // Filter functions based on current filter
    let filteredFunctions = availableFunctions;
    if (currentFunctionFilter !== 'All') {
        filteredFunctions = availableFunctions.filter(f => 
            (f.ExecutionType || f.executionType) === currentFunctionFilter
        );
    }
    
    countEl.textContent = `(${filteredFunctions.length})`;
    
    if (filteredFunctions.length === 0) {
        container.innerHTML = '<div style="padding: 0.75rem; text-align: center; color: #888; font-size: 0.8rem;">No functions for this type</div>';
        return;
    }
    
    const typeIcons = {
        'RestApi': 'fa-globe',
        'ScpiCommand': 'fa-broadcast-tower',
        'SdkMethod': 'fa-code'
    };
    
    container.innerHTML = filteredFunctions.map(func => {
        const icon = typeIcons[func.ExecutionType || func.executionType] || 'fa-cog';
        const name = func.Name || func.name;
        const type = func.ExecutionType || func.executionType;
        const funcId = func.FunctionId || func.functionId;
        
        return `
            <div class="tool-item" draggable="true" 
                 data-type="function-operation" 
                 data-function-id="${func.Id || func.id}"
                 data-function-def-id="${funcId}"
                 data-operation-type="${type}"
                 data-function-name="${name}"
                 data-function-desc="${func.Description || func.description || ''}"
                 title="${func.Description || func.description || name}">
                <i class="fas ${icon}"></i>
                <div class="tool-label" style="flex: 1;">
                    <div style="font-weight: 500;">${name}</div>
                    <div style="font-size: 0.7rem; opacity: 0.8;">${type}</div>
                </div>
                <i class="fas fa-link" style="font-size: 0.7rem; opacity: 0.6;"></i>
            </div>
        `;
    }).join('');
    
    // Re-initialize drag for new elements
    container.querySelectorAll('.tool-item').forEach(item => {
        item.addEventListener('dragstart', handleToolDragStart);
    });
}

function filterFunctions(type) {
    currentFunctionFilter = type;
    
    // Update button styles
    document.querySelectorAll('.filter-btn').forEach(btn => {
        if (btn.dataset.filter === type) {
            btn.classList.add('active');
            btn.style.background = '#4CAF50';
            btn.style.color = 'white';
        } else {
            btn.classList.remove('active');
            btn.style.background = 'white';
            btn.style.color = '#666';
        }
    });
    
    renderAvailableFunctions();
}

function initializeDragAndDrop() {
    const toolItems = document.querySelectorAll('.tool-item');
    toolItems.forEach(item => {
        item.addEventListener('dragstart', handleToolDragStart);
    });

    const dropZones = document.querySelectorAll('.drop-zone');
    dropZones.forEach(zone => {
        zone.addEventListener('dragover', handleDragOver);
        zone.addEventListener('drop', handleDrop);
        zone.addEventListener('dragleave', handleDragLeave);
    });
}

function handleToolDragStart(e) {
    const target = e.target.closest('.tool-item');
    if (!target) return;
    
    draggedElement = {
        type: target.dataset.type,
        operationType: target.dataset.operationType,
        functionId: target.dataset.functionId,
        functionDefId: target.dataset.functionDefId,
        functionName: target.dataset.functionName,
        functionDesc: target.dataset.functionDesc
    };
    e.dataTransfer.effectAllowed = 'copy';
    e.dataTransfer.setData('text/plain', target.dataset.type);
}

function handleDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    
    e.currentTarget.classList.add('drag-over');
    e.dataTransfer.dropEffect = 'copy';
    return false;
}

function handleDragLeave(e) {
    if (e.target === e.currentTarget) {
        e.currentTarget.classList.remove('drag-over');
    }
}

async function handleDrop(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    
    e.currentTarget.classList.remove('drag-over');
    
    // Get zone from ID (setupZone, stepsZone, teardownZone, step-X-actions, action-X-pre/operations/post)
    const zoneId = e.currentTarget.id;
    let zone = 'steps'; // default
    
    if (zoneId === 'setupZone') {
        zone = 'setup';
    } else if (zoneId === 'stepsZone') {
        zone = 'steps';
    } else if (zoneId === 'teardownZone') {
        zone = 'teardown';
    } else if (zoneId && zoneId.startsWith('step-') && zoneId.endsWith('-actions')) {
        // Extract step ID from "step-X-actions"
        zone = zoneId.replace('-actions', '');
    } else if (zoneId && zoneId.startsWith('action-')) {
        // Handle action zones: action-X-pre, action-X-operations, action-X-post
        if (zoneId.endsWith('-pre')) {
            zone = 'action-pre-' + zoneId.match(/action-(\d+)-pre/)[1];
        } else if (zoneId.endsWith('-operations')) {
            zone = 'action-' + zoneId.match(/action-(\d+)-operations/)[1];
        } else if (zoneId.endsWith('-post')) {
            zone = 'action-post-' + zoneId.match(/action-(\d+)-post/)[1];
        }
    }
    
    if (draggedElement) {
        if (draggedElement.type === 'function-operation') {
            // Dragged a linked function - create operation with function data
            console.log('Dropping function-operation:', draggedElement);
            const functionData = {
                Id: draggedElement.functionId,
                FunctionId: draggedElement.functionDefId,
                Name: draggedElement.functionName,
                Description: draggedElement.functionDesc,
                ExecutionType: draggedElement.operationType
            };
            console.log('Creating operation with functionData:', functionData);
            createElement('operation', zone, draggedElement.operationType, e.currentTarget, functionData);
            updateDropZoneState(e.currentTarget);
            updateAllCounts();
        } else if (draggedElement.type.startsWith('operation-')) {
            const operationType = draggedElement.type.replace('operation-', '');
            await selectFunctionForOperation(operationType, zone, e.currentTarget);
        } else {
            createElement(draggedElement.type, zone, null, e.currentTarget);
            updateDropZoneState(e.currentTarget);
            updateAllCounts();
        }
    }
    
    return false;
}

async function selectFunctionForOperation(operationType, zone, dropZoneElement) {
    const typeMapping = {
        'restapi': 'RestApi',
        'scpi': 'ScpiCommand',
        'sdkmethod': 'SdkMethod',
        'ssh': 'Ssh'
    };
    
    const executionType = typeMapping[operationType.toLowerCase()];
    const filteredFunctions = availableFunctions.filter(f => {
        const fType = f.ExecutionType || f.executionType || f.execution_type;
        return fType === executionType;
    });
    
    console.log(`Filtering for ${operationType} (${executionType}):`, {
        totalFunctions: availableFunctions.length,
        filteredCount: filteredFunctions.length,
        availableTypes: availableFunctions.map(f => f.ExecutionType || f.executionType || f.execution_type)
    });
    
    if (filteredFunctions.length === 0) {
        if (confirm(`No ${operationType} functions found. Create empty operation?`)) {
            createElement('operation', zone, operationType, dropZoneElement);
            updateDropZoneState(dropZoneElement);
            updateAllCounts();
        }
        return;
    }
    
    showFunctionSelectionModal(filteredFunctions, operationType, zone, dropZoneElement);
}

function showFunctionSelectionModal(functions, operationType, zone, dropZoneElement) {
    const modal = document.getElementById('functionModal');
    const modalTitle = modal.querySelector('.modal-header h3');
    const modalBody = modal.querySelector('.modal-body #functionList');
    const modalFooter = modal.querySelector('.modal-footer');
    
    modalTitle.innerHTML = `<i class="fas fa-list"></i> Select ${operationType.toUpperCase()} Function`;
    
    let html = `
        <div class="property-group">
            <label>Available Functions (${functions.length})</label>
            <div style="max-height: 400px; overflow-y: auto; border: 2px solid #e0e0e0; border-radius: 8px;">
    `;
    
    functions.forEach(func => {
        const funcName = func.Name || func.name || 'Unnamed Function';
        const funcDesc = func.Description || func.description || 'No description';
        const funcId = func.FunctionId || func.functionId || func.function_id || 'N/A';
        const funcDbId = func.Id || func.id;
        const execType = func.ExecutionType || func.executionType || func.execution_type || operationType;
        
        html += `
            <div class="function-item" style="padding: 15px; border-bottom: 1px solid #e0e0e0; cursor: pointer; transition: all 0.3s;" 
                 onmouseover="this.style.background='#f8f9ff'" 
                 onmouseout="this.style.background='white'"
                 onclick="selectFunction('${funcDbId}', '${operationType}', '${zone}')">
                <div style="display: flex; justify-content: space-between; align-items: start;">
                    <div style="flex: 1;">
                        <div style="font-weight: 600; color: #333; margin-bottom: 5px;">
                            <i class="fas fa-cube"></i> ${funcName}
                        </div>
                        <div style="font-size: 0.85rem; color: #666; margin-bottom: 5px;">
                            ${funcDesc}
                        </div>
                        <div style="font-size: 0.8rem; color: #999;">
                            <strong>Function ID:</strong> <code>${funcId}</code>
                        </div>
                    </div>
                    <span class="badge badge-${operationType.toLowerCase()}">${execType}</span>
                </div>
            </div>
        `;
    });
    
    html += `
            </div>
        </div>
        <div class="property-group" style="margin-top: 20px;">
            <button class="btn btn-secondary" style="width: 100%;" onclick="createEmptyOperation('${operationType}', '${zone}')">
                <i class="fas fa-plus"></i> Create Empty Operation (No Function)
            </button>
        </div>
    `;
    
    modalBody.innerHTML = html;
    modalFooter.style.display = 'none';
    modal.style.display = 'block';
}

function getDropZoneElement(zone) {
    // Handle main zones
    if (zone === 'setup') return document.getElementById('setupZone');
    if (zone === 'steps') return document.getElementById('stepsZone');
    if (zone === 'teardown') return document.getElementById('teardownZone');
    
    // Handle step actions zone
    if (zone.startsWith('step-') && !zone.includes('-actions')) {
        return document.getElementById(`${zone}-actions`);
    }
    
    // Handle action zones
    if (zone.startsWith('action-pre-')) {
        const actionId = zone.replace('action-pre-', '');
        return document.getElementById(`action-${actionId}-pre`);
    }
    if (zone.startsWith('action-post-')) {
        const actionId = zone.replace('action-post-', '');
        return document.getElementById(`action-${actionId}-post`);
    }
    if (zone.startsWith('action-')) {
        const actionId = zone.replace('action-', '');
        return document.getElementById(`action-${actionId}-operations`);
    }
    
    // Default: try direct ID lookup
    return document.getElementById(zone);
}

function selectFunction(functionId, operationType, zone) {
    const selectedFunc = availableFunctions.find(f => (f.Id || f.id) === functionId);
    if (selectedFunc) {
        const normalizedFunc = {
            Id: selectedFunc.Id || selectedFunc.id,
            FunctionId: selectedFunc.FunctionId || selectedFunc.functionId || selectedFunc.function_id,
            Name: selectedFunc.Name || selectedFunc.name,
            Description: selectedFunc.Description || selectedFunc.description,
            ExecutionType: selectedFunc.ExecutionType || selectedFunc.executionType || selectedFunc.execution_type
        };
        console.log('Selected function for operation:', normalizedFunc);
        const dropZone = getDropZoneElement(zone);
        if (dropZone) {
            createElement('operation', zone, operationType, dropZone, normalizedFunc);
            updateDropZoneState(dropZone);
            updateAllCounts();
        } else {
            console.error('Could not find drop zone for:', zone);
            showNotification('✗ Error: Could not find drop zone', 'error');
        }
    }
    closeFunctionModal();
}

function createEmptyOperation(operationType, zone) {
    const dropZone = getDropZoneElement(zone);
    if (dropZone) {
        createElement('operation', zone, operationType, dropZone, null);
        updateDropZoneState(dropZone);
        updateAllCounts();
    } else {
        console.error('Could not find drop zone for:', zone);
        showNotification('✗ Error: Could not find drop zone', 'error');
    }
    closeFunctionModal();
}

function createElement(type, zone, operationType = null, dropZoneElement = null, functionData = null) {
    const id = ++elementCounter;
    let element;

    switch(type) {
        case 'step':
            if (zone === 'steps') {
                element = createStepElement(id);
                const stepsZone = dropZoneElement || document.getElementById('stepsZone');
                stepsZone.appendChild(element);
            }
            break;
        case 'action':
            if (zone.startsWith('step-')) {
                element = createActionElement(id);
                const actionsZone = dropZoneElement || document.querySelector(`#${zone}-actions`);
                if (actionsZone) {
                    actionsZone.appendChild(element);
                    updateDropZoneState(actionsZone);
                }
            }
            break;
        case 'operation':
            element = createOperationElement(id, operationType, zone, functionData);
            let opZone;
            if (zone === 'setup') {
                opZone = dropZoneElement || document.getElementById('setupZone');
            } else if (zone === 'teardown') {
                opZone = dropZoneElement || document.getElementById('teardownZone');
            } else if (zone.startsWith('action-pre-')) {
                const actionId = zone.replace('action-pre-', '');
                opZone = dropZoneElement || document.getElementById(`action-${actionId}-pre`);
            } else if (zone.startsWith('action-post-')) {
                const actionId = zone.replace('action-post-', '');
                opZone = dropZoneElement || document.getElementById(`action-${actionId}-post`);
            } else if (zone.startsWith('action-')) {
                const actionId = zone.replace('action-', '');
                opZone = dropZoneElement || document.getElementById(`action-${actionId}-operations`);
            }
            
            if (opZone) {
                opZone.appendChild(element);
                updateDropZoneState(opZone);
            }
            break;
    }

    if (element) {
        reorderElements(zone);
    }
}

function createStepElement(id) {
    const div = document.createElement('div');
    div.className = 'test-step';
    div.dataset.id = id;
    div.dataset.type = 'step';
    div.innerHTML = `
        <div class="element-header" draggable="true">
            <div class="element-title">
                <span class="element-order">1</span>
                <i class="fas fa-list-ol"></i>
                <span class="element-name" id="step-name-${id}">Step ${id}</span>
            </div>
            <div class="element-controls">
                <button class="control-btn move-up" onclick="moveElement(${id}, 'up')" title="Move Up">
                    <i class="fas fa-arrow-up"></i>
                </button>
                <button class="control-btn move-down" onclick="moveElement(${id}, 'down')" title="Move Down">
                    <i class="fas fa-arrow-down"></i>
                </button>
                <button class="control-btn toggle" onclick="toggleElement(${id})" title="Collapse/Expand">
                    <i class="fas fa-minus"></i>
                </button>
                <button class="control-btn edit" onclick="editElement(${id}, 'step')" title="Edit">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="control-btn delete" onclick="deleteElement(${id})" title="Delete">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
        <div class="element-body" id="body-${id}">
            <div class="element-detail">
                <strong>Description:</strong>
                <span id="step-desc-${id}">Step description</span>
            </div>
            <div class="element-detail">
                <strong>Delay Before:</strong>
                <span id="step-delay-${id}">0</span>ms
            </div>
            <div class="element-detail">
                <strong>Retry Count:</strong>
                <span id="step-retry-${id}">0</span>
            </div>
            <div class="subsection">
                <h4><i class="fas fa-bolt"></i> Actions (<span class="count-actions-${id}">0</span>)</h4>
                <div class="drop-zone empty" id="step-${id}-actions"></div>
            </div>
        </div>
    `;
    
    const dropZone = div.querySelector('.drop-zone');
    dropZone.addEventListener('dragover', handleDragOver);
    dropZone.addEventListener('drop', handleDrop);
    dropZone.addEventListener('dragleave', handleDragLeave);
    
    return div;
}

function createActionElement(id) {
    const div = document.createElement('div');
    div.className = 'test-action';
    div.dataset.id = id;
    div.dataset.type = 'action';
    div.innerHTML = `
        <div class="element-header" draggable="true">
            <div class="element-title">
                <span class="element-order">1</span>
                <i class="fas fa-bolt"></i>
                <span class="element-name" id="action-name-${id}">Action ${id}</span>
            </div>
            <div class="element-controls">
                <button class="control-btn move-up" onclick="moveElement(${id}, 'up')" title="Move Up">
                    <i class="fas fa-arrow-up"></i>
                </button>
                <button class="control-btn move-down" onclick="moveElement(${id}, 'down')" title="Move Down">
                    <i class="fas fa-arrow-down"></i>
                </button>
                <button class="control-btn toggle" onclick="toggleElement(${id})" title="Collapse/Expand">
                    <i class="fas fa-minus"></i>
                </button>
                <button class="control-btn edit" onclick="editElement(${id}, 'action')" title="Edit">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="control-btn delete" onclick="deleteElement(${id})" title="Delete">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
        <div class="element-body" id="body-${id}">
            <div class="element-detail">
                <strong>Description:</strong>
                <span id="action-desc-${id}">Action description</span>
            </div>
            <div class="element-detail">
                <strong>Delay Before:</strong>
                <span id="action-delay-before-${id}">0</span>ms
            </div>
            <div class="element-detail">
                <strong>Delay After:</strong>
                <span id="action-delay-after-${id}">0</span>ms
            </div>
            <div class="element-detail">
                <strong>Retry Count:</strong>
                <span id="action-retry-${id}">0</span>
            </div>
            
            <div class="subsection">
                <h4><i class="fas fa-check-circle"></i> Pre-Conditions (<span class="count-pre-${id}">0</span>)</h4>
                <div class="drop-zone empty" id="action-${id}-pre"></div>
            </div>
            
            <div class="subsection">
                <h4><i class="fas fa-cog"></i> Operations (<span class="count-op-${id}">0</span>)</h4>
                <div class="drop-zone empty" id="action-${id}-operations"></div>
            </div>
            
            <div class="subsection">
                <h4><i class="fas fa-clipboard-check"></i> Post-Conditions (<span class="count-post-${id}">0</span>)</h4>
                <div class="drop-zone empty" id="action-${id}-post"></div>
            </div>
        </div>
    `;
    
    const dropZones = div.querySelectorAll('.drop-zone');
    dropZones.forEach(zone => {
        zone.addEventListener('dragover', handleDragOver);
        zone.addEventListener('drop', handleDrop);
        zone.addEventListener('dragleave', handleDragLeave);
    });
    
    return div;
}

function createOperationElement(id, operationType, zone, functionData = null) {
    const typeIcons = {
        'RestApi': 'fa-globe',
        'Scpi': 'fa-broadcast-tower',
        'ScpiCommand': 'fa-broadcast-tower',
        'SdkMethod': 'fa-code',
        'Ssh': 'fa-terminal'
    };
    
    const div = document.createElement('div');
    div.className = 'test-operation';
    div.dataset.id = id;
    div.dataset.type = 'operation';
    div.dataset.operationType = operationType;
    
    if (functionData) {
        // Use MongoDB document ID (Id or id) as FunctionDefinitionId
        div.dataset.functionId = functionData.Id || functionData.id;
        div.dataset.functionDefId = functionData.FunctionId || functionData.functionId;
    }
    
    const name = functionData ? (functionData.Name || functionData.name) : `${operationType} Operation`;
    const description = functionData ? (functionData.Description || functionData.description || 'No description') : 'Operation description';
    const functionIdDisplay = functionData ? (functionData.FunctionId || functionData.functionId) : 'Not linked';
    
    div.innerHTML = `
        <div class="element-header" draggable="true">
            <div class="element-title">
                <span class="element-order">1</span>
                <i class="fas ${typeIcons[operationType]}"></i>
                <span class="element-name" id="op-name-${id}">${name}</span>
                <span class="badge badge-${operationType.toLowerCase()}">${operationType}</span>
                ${functionData ? '<span class="badge" style="background: #28a745; margin-left: 5px;"><i class="fas fa-link"></i> Linked</span>' : ''}
            </div>
            <div class="element-controls">
                <button class="control-btn move-up" onclick="moveElement(${id}, 'up')" title="Move Up">
                    <i class="fas fa-arrow-up"></i>
                </button>
                <button class="control-btn move-down" onclick="moveElement(${id}, 'down')" title="Move Down">
                    <i class="fas fa-arrow-down"></i>
                </button>
                <button class="control-btn toggle" onclick="toggleElement(${id})" title="Collapse/Expand">
                    <i class="fas fa-minus"></i>
                </button>
                <button class="control-btn edit" onclick="editElement(${id}, 'operation', '${operationType}')" title="Edit">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="control-btn delete" onclick="deleteElement(${id})" title="Delete">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
        <div class="element-body" id="body-${id}">
            <div class="element-detail">
                <strong>Type:</strong>
                <span>${operationType}</span>
            </div>
            <div class="element-detail">
                <strong>Function ID:</strong>
                <span id="op-function-id-${id}" style="color: ${functionData ? '#28a745' : '#999'};">${functionIdDisplay}</span>
            </div>
            <div class="element-detail">
                <strong>Timeout:</strong>
                <span id="op-timeout-${id}">30</span>s
            </div>
            <div class="element-detail">
                <strong>Retry Count:</strong>
                <span id="op-retry-${id}">0</span>
            </div>
            <div class="element-detail">
                <strong>Retry Delay:</strong>
                <span id="op-retry-delay-${id}">1000</span>ms
            </div>
            <div class="element-detail">
                <strong>Description:</strong>
                <span id="op-desc-${id}">${description}</span>
            </div>
        </div>
    `;
    
    return div;
}

function moveElement(id, direction) {
    const element = document.querySelector(`[data-id="${id}"]`);
    const parent = element.parentElement;
    const siblings = Array.from(parent.children).filter(el => 
        el.classList.contains(element.className.split(' ')[0])
    );
    
    const currentIndex = siblings.indexOf(element);
    
    if (direction === 'up' && currentIndex > 0) {
        parent.insertBefore(element, siblings[currentIndex - 1]);
    } else if (direction === 'down' && currentIndex < siblings.length - 1) {
        parent.insertBefore(siblings[currentIndex + 1], element);
    }
    
    reorderElements(parent.dataset.zone);
}

function reorderElements(zone) {
    const zoneElement = document.querySelector(`[data-zone="${zone}"]`);
    if (!zoneElement) return;
    
    const elements = Array.from(zoneElement.children).filter(el => 
        el.classList.contains('test-step') || 
        el.classList.contains('test-action') || 
        el.classList.contains('test-operation')
    );
    
    elements.forEach((el, index) => {
        const orderBadge = el.querySelector('.element-order');
        if (orderBadge) {
            orderBadge.textContent = index + 1;
        }
    });
    
    updateAllCounts();
}

function toggleElement(id) {
    const body = document.getElementById(`body-${id}`);
    const btn = event.target.closest('button');
    const icon = btn.querySelector('i');
    
    if (body.classList.contains('collapsed')) {
        body.classList.remove('collapsed');
        icon.className = 'fas fa-minus';
    } else {
        body.classList.add('collapsed');
        icon.className = 'fas fa-plus';
    }
}

function deleteElement(id) {
    if (confirm('Are you sure you want to delete this element and all its children?')) {
        const element = document.querySelector(`[data-id="${id}"]`);
        const parent = element.parentElement;
        element.remove();
        
        if (parent.dataset.zone) {
            reorderElements(parent.dataset.zone);
            updateDropZoneState(parent);
        }
        updateAllCounts();
    }
}

function updateDropZoneState(zone) {
    if (!zone) {
        console.warn('updateDropZoneState called with null zone');
        return;
    }
    
    const hasChildren = Array.from(zone.children).some(el => 
        el.classList.contains('test-step') || 
        el.classList.contains('test-action') || 
        el.classList.contains('test-operation')
    );
    
    if (hasChildren) {
        zone.classList.remove('empty');
    } else {
        zone.classList.add('empty');
    }
}

function updateAllCounts() {
    const setupZone = document.getElementById('setupZone');
    const setupCount = setupZone ? setupZone.querySelectorAll('.test-operation').length : 0;
    const setupCountEl = document.getElementById('setupCount');
    if (setupCountEl) setupCountEl.textContent = setupCount;
    
    const stepsZone = document.getElementById('stepsZone');
    const stepsCount = stepsZone ? stepsZone.querySelectorAll('.test-step').length : 0;
    const stepsCountEl = document.getElementById('stepsCount');
    if (stepsCountEl) stepsCountEl.textContent = stepsCount;
    
    const teardownZone = document.getElementById('teardownZone');
    const teardownCount = teardownZone ? teardownZone.querySelectorAll('.test-operation').length : 0;
    const teardownCountEl = document.getElementById('teardownCount');
    if (teardownCountEl) teardownCountEl.textContent = teardownCount;
    
    document.querySelectorAll('.test-step').forEach(step => {
        const stepId = step.dataset.id;
        const actionsZone = document.getElementById(`step-${stepId}-actions`);
        const actionsCount = actionsZone ? actionsZone.querySelectorAll('.test-action').length : 0;
        const countEl = step.querySelector(`.count-actions-${stepId}`);
        if (countEl) countEl.textContent = actionsCount;
    });
    
    document.querySelectorAll('.test-action').forEach(action => {
        const actionId = action.dataset.id;
        
        const preZone = document.getElementById(`action-${actionId}-pre`);
        const preCount = preZone ? preZone.querySelectorAll('.test-operation').length : 0;
        const preCountEl = action.querySelector(`.count-pre-${actionId}`);
        if (preCountEl) preCountEl.textContent = preCount;
        
        const opZone = document.getElementById(`action-${actionId}-operations`);
        const opCount = opZone ? opZone.querySelectorAll('.test-operation').length : 0;
        const opCountEl = action.querySelector(`.count-op-${actionId}`);
        if (opCountEl) opCountEl.textContent = opCount;
        
        const postZone = document.getElementById(`action-${actionId}-post`);
        const postCount = postZone ? postZone.querySelectorAll('.test-operation').length : 0;
        const postCountEl = action.querySelector(`.count-post-${actionId}`);
        if (postCountEl) postCountEl.textContent = postCount;
    });
}

function editElement(id, type, operationType = null) {
    selectedElement = { id, type, operationType };
    const modal = document.getElementById('editModal');
    const modalTitle = modal.querySelector('.modal-header h3 span');
    const modalBody = modal.querySelector('.modal-body');
    
    let icon = '';
    if (type === 'step') icon = '<i class="fas fa-list-ol"></i>';
    else if (type === 'action') icon = '<i class="fas fa-bolt"></i>';
    else icon = '<i class="fas fa-cog"></i>';
    
    modalTitle.innerHTML = `Edit ${type.charAt(0).toUpperCase() + type.slice(1)}`;
    
    let formHTML = '';
    
    switch(type) {
        case 'step':
            const stepName = document.getElementById(`step-name-${id}`)?.textContent || 'New Step';
            const stepDesc = document.getElementById(`step-desc-${id}`)?.textContent || '';
            const stepDelay = document.getElementById(`step-delay-${id}`)?.textContent || '0';
            const stepRetry = document.getElementById(`step-retry-${id}`)?.textContent || '0';
            formHTML = `
                <div class="property-group">
                    <label>Step Name *</label>
                    <input type="text" id="edit-name" value="${stepName}" placeholder="Enter step name" />
                </div>
                <div class="property-group">
                    <label>Description</label>
                    <textarea id="edit-description" rows="3" placeholder="Describe what this step does">${stepDesc}</textarea>
                </div>
                <div class="form-row">
                    <div class="property-group">
                        <label>Delay Before (ms)</label>
                        <input type="number" id="edit-delay" value="${stepDelay}" min="0" />
                    </div>
                    <div class="property-group">
                        <label>Retry Count</label>
                        <input type="number" id="edit-retry" value="${stepRetry}" min="0" max="10" />
                    </div>
                </div>
                <div class="property-group">
                    <div class="checkbox-group">
                        <input type="checkbox" id="edit-continue" />
                        <label>Continue test execution if this step fails</label>
                    </div>
                </div>
            `;
            break;
            
        case 'action':
            const actionName = document.getElementById(`action-name-${id}`)?.textContent || 'New Action';
            const actionDesc = document.getElementById(`action-desc-${id}`)?.textContent || '';
            const delayBefore = document.getElementById(`action-delay-before-${id}`)?.textContent || '0';
            const delayAfter = document.getElementById(`action-delay-after-${id}`)?.textContent || '0';
            const actionRetry = document.getElementById(`action-retry-${id}`)?.textContent || '0';
            formHTML = `
                <div class="property-group">
                    <label>Action Name *</label>
                    <input type="text" id="edit-name" value="${actionName}" placeholder="Enter action name" />
                </div>
                <div class="property-group">
                    <label>Description</label>
                    <textarea id="edit-description" rows="2" placeholder="Describe what this action does">${actionDesc}</textarea>
                </div>
                <div class="form-row">
                    <div class="property-group">
                        <label>Delay Before (ms)</label>
                        <input type="number" id="edit-delay-before" value="${delayBefore}" min="0" />
                    </div>
                    <div class="property-group">
                        <label>Delay After (ms)</label>
                        <input type="number" id="edit-delay-after" value="${delayAfter}" min="0" />
                    </div>
                </div>
                <div class="property-group">
                    <label>Retry Count</label>
                    <input type="number" id="edit-retry" value="${actionRetry}" min="0" max="10" />
                </div>
                <div class="property-group">
                    <div class="checkbox-group">
                        <input type="checkbox" id="edit-continue" />
                        <label>Continue if this action fails</label>
                    </div>
                </div>
            `;
            break;
            
        case 'operation':
            const opElement = document.querySelector(`[data-id="${id}"]`);
            const currentFunctionId = opElement?.dataset.functionId || '';
            const currentFunctionDefId = opElement?.dataset.functionDefId || '';
            
            const opName = document.getElementById(`op-name-${id}`)?.textContent?.replace(/RestApi|Scpi|SdkMethod|Ssh|Linked/g, '').trim() || 'New Operation';
            const opDesc = document.getElementById(`op-desc-${id}`)?.textContent || '';
            const opTimeout = document.getElementById(`op-timeout-${id}`)?.textContent || '30';
            const opRetry = document.getElementById(`op-retry-${id}`)?.textContent || '0';
            const opRetryDelay = document.getElementById(`op-retry-delay-${id}`)?.textContent || '1000';
            
            const typeMapping = {
                'RestApi': 'RestApi',
                'Scpi': 'ScpiCommand',
                'SdkMethod': 'SdkMethod',
                'Ssh': 'Ssh'
            };
            const executionType = typeMapping[operationType];
            const filteredFunctions = availableFunctions.filter(f => 
                (f.executionType || f.execution_type) === executionType
            );
            
            let functionOptions = '<option value="">-- No Function (Manual Config) --</option>';
            filteredFunctions.forEach(func => {
                const funcId = func.id || func.Id;
                const funcName = func.name || func.Name || 'Unnamed';
                const funcFuncId = func.functionId || func.function_id || 'N/A';
                const selected = funcId === currentFunctionId ? 'selected' : '';
                functionOptions += `<option value="${funcId}" ${selected}>${funcName} (${funcFuncId})</option>`;
            });
            
            formHTML = `
                <div class="property-group">
                    <label>Link to Function Definition</label>
                    <select id="edit-function-link" onchange="loadFunctionDetails(this.value, ${id})">
                        ${functionOptions}
                    </select>
                    <small style="color: #666; margin-top: 5px; display: block;">
                        Select a function to auto-fill details, or choose "No Function" for manual configuration
                    </small>
                </div>
                <div class="property-group">
                    <label>Operation Name *</label>
                    <input type="text" id="edit-name" value="${opName}" placeholder="Enter operation name" />
                </div>
                <div class="property-group">
                    <label>Description</label>
                    <textarea id="edit-description" rows="2" placeholder="Describe what this operation does">${opDesc}</textarea>
                </div>
                <div class="form-row">
                    <div class="property-group">
                        <label>Timeout (seconds)</label>
                        <input type="number" id="edit-timeout" value="${opTimeout}" min="1" max="300" />
                    </div>
                    <div class="property-group">
                        <label>Current Function ID</label>
                        <input type="text" id="edit-function-id" value="${currentFunctionDefId}" readonly style="background: #f0f0f0;" />
                    </div>
                </div>
                <div class="form-row">
                    <div class="property-group">
                        <label>Retry Count</label>
                        <input type="number" id="edit-retry" value="${opRetry}" min="0" max="10" />
                    </div>
                    <div class="property-group">
                        <label>Retry Delay (ms)</label>
                        <input type="number" id="edit-retry-delay" value="${opRetryDelay}" min="100" max="10000" />
                    </div>
                </div>
                <div class="property-group">
                    <label>Expected Result (for validation)</label>
                    <input type="text" id="edit-expected" placeholder="e.g., success, OK, 200" />
                </div>
                <div class="property-group">
                    <div class="checkbox-group">
                        <input type="checkbox" id="edit-continue" />
                        <label>Continue if this operation fails</label>
                    </div>
                </div>
            `;
            break;
    }
    
    modalBody.innerHTML = formHTML;
    modal.style.display = 'block';
}

function loadFunctionDetails(functionId, elementId) {
    if (!functionId) {
        const opElement = document.querySelector(`[data-id="${elementId}"]`);
        if (opElement) {
            delete opElement.dataset.functionId;
            delete opElement.dataset.functionDefId;
        }
        document.getElementById('edit-function-id').value = '';
        showNotification('ℹ Function link removed', 'success');
        return;
    }
    
    const func = availableFunctions.find(f => (f.id || f.Id) === functionId);
    if (func) {
        const funcName = func.name || func.Name || '';
        const funcDesc = func.description || func.Description || '';
        const funcFuncId = func.functionId || func.function_id || '';
        
        document.getElementById('edit-name').value = funcName;
        document.getElementById('edit-description').value = funcDesc;
        document.getElementById('edit-function-id').value = funcFuncId;
        
        const opElement = document.querySelector(`[data-id="${elementId}"]`);
        if (opElement) {
            opElement.dataset.functionId = func.id || func.Id;
            opElement.dataset.functionDefId = funcFuncId;
        }
        
        showNotification('✓ Function details loaded', 'success');
    }
}

function saveElement() {
    if (!selectedElement) return;
    
    const { id, type } = selectedElement;
    const name = document.getElementById('edit-name')?.value;
    const description = document.getElementById('edit-description')?.value;
    
    switch(type) {
        case 'step':
            if (name) document.getElementById(`step-name-${id}`).textContent = name;
            if (description) document.getElementById(`step-desc-${id}`).textContent = description;
            const stepDelay = document.getElementById('edit-delay')?.value;
            const stepRetry = document.getElementById('edit-retry')?.value;
            if (stepDelay) document.getElementById(`step-delay-${id}`).textContent = stepDelay;
            if (stepRetry) document.getElementById(`step-retry-${id}`).textContent = stepRetry;
            break;
        case 'action':
            if (name) document.getElementById(`action-name-${id}`).textContent = name;
            if (description) document.getElementById(`action-desc-${id}`).textContent = description;
            const delayBefore = document.getElementById('edit-delay-before')?.value;
            const delayAfter = document.getElementById('edit-delay-after')?.value;
            const actionRetry = document.getElementById('edit-retry')?.value;
            if (delayBefore) document.getElementById(`action-delay-before-${id}`).textContent = delayBefore;
            if (delayAfter) document.getElementById(`action-delay-after-${id}`).textContent = delayAfter;
            if (actionRetry) document.getElementById(`action-retry-${id}`).textContent = actionRetry;
            break;
        case 'operation':
            if (name) document.getElementById(`op-name-${id}`).textContent = name;
            if (description) document.getElementById(`op-desc-${id}`).textContent = description;
            const timeout = document.getElementById('edit-timeout')?.value;
            const opRetry = document.getElementById('edit-retry')?.value;
            const opRetryDelay = document.getElementById('edit-retry-delay')?.value;
            if (timeout) document.getElementById(`op-timeout-${id}`).textContent = timeout;
            if (opRetry) document.getElementById(`op-retry-${id}`).textContent = opRetry;
            if (opRetryDelay) document.getElementById(`op-retry-delay-${id}`).textContent = opRetryDelay;
            
            const functionDefId = document.getElementById('edit-function-id')?.value;
            const functionIdEl = document.getElementById(`op-function-id-${id}`);
            if (functionIdEl) {
                functionIdEl.textContent = functionDefId || 'Not linked';
                functionIdEl.style.color = functionDefId ? '#28a745' : '#999';
            }
            
            const opElement = document.querySelector(`[data-id="${id}"]`);
            const header = opElement?.querySelector('.element-header .element-title');
            if (header) {
                const existingBadge = header.querySelector('.badge[style*="background: #28a745"]');
                if (functionDefId && !existingBadge) {
                    const badge = document.createElement('span');
                    badge.className = 'badge';
                    badge.style.cssText = 'background: #28a745; margin-left: 5px;';
                    badge.innerHTML = '<i class="fas fa-link"></i> Linked';
                    header.appendChild(badge);
                } else if (!functionDefId && existingBadge) {
                    existingBadge.remove();
                }
            }
            break;
    }
    
    closeEditModal();
    showNotification('✓ Changes saved successfully', 'success');
}

function closeModal() {
    closeEditModal();
}

function closeEditModal() {
    const modal = document.getElementById('editModal');
    const modalFooter = modal.querySelector('.modal-footer');
    modal.style.display = 'none';
    modalFooter.style.display = 'flex';
    selectedElement = null;
}

function closeFunctionModal() {
    document.getElementById('functionModal').style.display = 'none';
}

async function saveTestCase() {
    // Get values from form fields (note: HTML uses lowercase IDs like 'testcaseName')
    const testCaseIdField = document.getElementById('testCaseId');
    const nameField = document.getElementById('testCaseName');
    const descField = document.getElementById('testCaseDescription');
    const priorityField = document.getElementById('testCasePriority');
    const categoryField = document.getElementById('testCaseCategory');
    const tagsField = document.getElementById('testCaseTags');
    
    testCaseData.Name = nameField ? nameField.value : 'Untitled Test Case';
    testCaseData.Description = descField ? descField.value : '';
    testCaseData.Priority = priorityField ? priorityField.value : 'Medium';
    testCaseData.Category = categoryField ? categoryField.value : 'Functional';
    
    // Build tags array from any additional tags
    const tags = [];
    if (tagsField && tagsField.value) {
        tags.push(...tagsField.value.split(',').map(t => t.trim()).filter(t => t));
    }
    testCaseData.Tags = tags;
    
    testCaseData.Steps = collectSteps();
    testCaseData.SetupOperations = collectOperations('setupZone');
    testCaseData.TeardownOperations = collectOperations('teardownZone');
    
    console.log('Test Case Data to save:', testCaseData);
    console.log('Steps:', testCaseData.Steps.length);
    console.log('Setup Ops:', testCaseData.SetupOperations.length);
    console.log('Teardown Ops:', testCaseData.TeardownOperations.length);
    
    // Check if this is an update or create
    const isUpdate = testCaseIdField && testCaseIdField.value;
    const testCaseId = isUpdate ? testCaseIdField.value : null;
    
    try {
        const url = isUpdate ? `/api/testcases/${testCaseId}` : '/api/testcases';
        const method = isUpdate ? 'PUT' : 'POST';
        
        console.log(`${isUpdate ? 'Updating' : 'Creating'} test case:`, url);
        
        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(testCaseData)
        });
        
        if (response.ok) {
            const result = await response.json();
            
            if (isUpdate) {
                showNotification(`✓ Test case updated successfully!`, 'success');
                // Optionally redirect back to list after short delay
                setTimeout(() => {
                    if (confirm('Test case updated! Go back to Test Cases list?')) {
                        window.location.href = '/TestCases';
                    }
                }, 1000);
            } else {
                showNotification(`✓ Test case saved successfully!`, 'success');
            }
            
            console.log('Saved result:', result);
            console.log('Saved Steps:', result.steps?.length || 0);
            console.log('Saved Setup:', result.setupOperations?.length || 0);
            
            // Update the hidden ID field if this was a create
            if (!isUpdate && result.id) {
                testCaseIdField.value = result.id;
                window.history.pushState({}, '', `?id=${result.id}`);
                
                // Switch to edit mode UI
                const editBanner = document.getElementById('editModeBanner');
                const editingName = document.getElementById('editingTestCaseName');
                const saveButton = document.getElementById('saveButton');
                const saveButtonText = document.getElementById('saveButtonText');
                const cancelButton = document.getElementById('cancelButton');
                
                if (editBanner) {
                    editBanner.style.display = 'block';
                    if (editingName) editingName.textContent = testCaseData.Name;
                }
                if (saveButton) {
                    saveButton.classList.add('edit-mode');
                    if (saveButtonText) saveButtonText.textContent = 'Update Test Case';
                }
                if (cancelButton) cancelButton.style.display = 'inline-flex';
            }
        } else {
            const error = await response.text();
            showNotification(`✗ Failed to ${isUpdate ? 'update' : 'save'}: ` + error, 'error');
            console.error('Save error:', error);
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('✗ Error: ' + error.message, 'error');
    }
}

async function executeTestCase() {
    testCaseData.Name = document.getElementById('testCaseName').value || 'Untitled Test Case';
    testCaseData.Description = document.getElementById('testCaseDescription').value || '';
    testCaseData.Steps = collectSteps();
    testCaseData.SetupOperations = collectOperations('setup');
    testCaseData.TeardownOperations = collectOperations('teardown');
    
    console.log('=== TEST CASE DATA BEFORE EXECUTION ===');
    console.log('Name:', testCaseData.Name);
    console.log('Steps:', testCaseData.Steps?.length || 0);
    console.log('Setup Operations:', testCaseData.SetupOperations?.length || 0);
    console.log('Teardown Operations:', testCaseData.TeardownOperations?.length || 0);
    console.log('Full testCaseData:', JSON.stringify(testCaseData, null, 2));
    console.log('======================================');
    
    try {
        showNotification('⏳ Executing test case...', 'success');
        const response = await fetch('/api/testexecution/execute', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(testCaseData)
        });
        
        if (response.ok) {
            const result = await response.json();
            showNotification(`✓ Execution ${result.success ? 'passed' : 'failed'} in ${result.totalExecutionTimeMs}ms`, result.success ? 'success' : 'error');
            console.log('Execution result:', result);
            alert(`Test Execution Complete!\n\nStatus: ${result.status}\nSuccess: ${result.success}\nTime: ${result.totalExecutionTimeMs}ms\n\nCheck console for detailed results.`);
        } else {
            showNotification('✗ Execution failed', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showNotification('✗ Execution error: ' + error.message, 'error');
    }
}

function collectSteps() {
    const steps = [];
    const stepsZone = document.getElementById('stepsZone');
    if (!stepsZone) {
        console.warn('stepsZone not found');
        return steps;
    }
    
    const stepElements = stepsZone.querySelectorAll(':scope > .test-step');
    console.log('Found step elements:', stepElements.length);
    
    stepElements.forEach((stepEl, index) => {
        const id = stepEl.dataset.id;
        const step = {
            Name: document.getElementById(`step-name-${id}`)?.textContent || `Step ${index + 1}`,
            Description: document.getElementById(`step-desc-${id}`)?.textContent || '',
            Order: index + 1,
            DelayBeforeMs: parseInt(document.getElementById(`step-delay-${id}`)?.textContent || '0'),
            RetryCount: parseInt(document.getElementById(`step-retry-${id}`)?.textContent || '0'),
            ContinueOnFailure: false,
            Actions: collectActions(id)
        };
        console.log(`Step ${id}:`, step.Name, 'Actions:', step.Actions.length);
        steps.push(step);
    });
    
    return steps;
}

function collectActions(stepId) {
    const actions = [];
    const actionsZone = document.getElementById(`step-${stepId}-actions`);
    if (!actionsZone) {
        console.warn(`Actions zone not found for step ${stepId}`);
        return actions;
    }
    
    const actionElements = actionsZone.querySelectorAll(':scope > .test-action');
    console.log(`Found action elements in step ${stepId}:`, actionElements.length);
    
    actionElements.forEach((actionEl, index) => {
        const id = actionEl.dataset.id;
        const action = {
            Name: document.getElementById(`action-name-${id}`)?.textContent || `Action ${index + 1}`,
            Description: document.getElementById(`action-desc-${id}`)?.textContent || '',
            Order: index + 1,
            DelayBeforeMs: parseInt(document.getElementById(`action-delay-before-${id}`)?.textContent || '0'),
            DelayAfterMs: parseInt(document.getElementById(`action-delay-after-${id}`)?.textContent || '0'),
            RetryCount: parseInt(document.getElementById(`action-retry-${id}`)?.textContent || '0'),
            ContinueOnFailure: false,
            PreConditions: collectOperations(`action-${id}-pre`),
            Operations: collectOperations(`action-${id}-operations`),
            PostConditions: collectOperations(`action-${id}-post`)
        };
        console.log(`Action ${id}:`, action.Name, 'PreConditions:', action.PreConditions.length, 'Operations:', action.Operations.length, 'PostConditions:', action.PostConditions.length);
        actions.push(action);
    });
    
    return actions;
}

function collectOperations(zoneId) {
    const operations = [];
    const zone = document.getElementById(zoneId);
    if (!zone) {
        console.warn(`Operations zone not found: ${zoneId}`);
        return operations;
    }
    
    const opElements = zone.querySelectorAll(':scope > .test-operation');
    console.log(`Found operation elements in ${zoneId}:`, opElements.length);
    
    opElements.forEach((opEl, index) => {
        const id = opEl.dataset.id;
        const operation = {
            Name: document.getElementById(`op-name-${id}`)?.textContent?.replace(/Linked/g, '').trim() || `Operation ${index + 1}`,
            Description: document.getElementById(`op-desc-${id}`)?.textContent || '',
            OperationType: opEl.dataset.operationType,
            Order: index + 1,
            TimeoutSeconds: parseInt(document.getElementById(`op-timeout-${id}`)?.textContent || '30'),
            RetryCount: parseInt(document.getElementById(`op-retry-${id}`)?.textContent || '0'),
            RetryDelayMs: parseInt(document.getElementById(`op-retry-delay-${id}`)?.textContent || '1000'),
            ContinueOnFailure: false,
            Parameters: {},
            OperationDetails: {}
        };
        
        if (opEl.dataset.functionId) {
            operation.FunctionDefinitionId = opEl.dataset.functionId;
        }
        
        console.log(`Operation ${id}:`, operation.Name, operation.OperationType);
        operations.push(operation);
    });
    
    return operations;
}

async function loadTestCasesList() {
    try {
        const response = await fetch('/api/testcases');
        if (response.ok) {
            const testCases = await response.json();
            if (testCases.length === 0) {
                alert('No test cases found. Create one or seed sample data first.');
                return;
            }
            
            let list = 'Available Test Cases:\n\n';
            testCases.forEach((tc, i) => {
                list += `${i + 1}. ${tc.name} (${tc.priority}) - ${tc.steps?.length || 0} steps\n`;
            });
            
            const choice = prompt(list + '\nEnter the number to load:');
            if (choice) {
                const index = parseInt(choice) - 1;
                if (index >= 0 && index < testCases.length) {
                    loadTestCase(testCases[index].id);
                }
            }
        }
    } catch (error) {
        console.error('Error loading test cases:', error);
        showNotification('✗ Failed to load test cases', 'error');
    }
}

async function loadTestCase(id) {
    try {
        const response = await fetch(`/api/testcases/${id}`);
        if (!response.ok) {
            showNotification('⚠ Failed to load test case', 'error');
            return;
        }
        
        const testCase = await response.json();
        console.log('Loaded test case data:', testCase);
        
        // Load basic fields
        document.getElementById('testCaseName').value = testCase.name || '';
        document.getElementById('testCaseDescription').value = testCase.description || '';
        document.getElementById('testCaseTags').value = testCase.tags?.join(', ') || '';
        document.getElementById('testCasePriority').value = testCase.priority || 'Medium';
        document.getElementById('testCaseCategory').value = testCase.category || 'Functional';
        
        // Load Setup Operations
        const setupZone = document.getElementById('setupZone');
        setupZone.innerHTML = '<p class="zone-placeholder">Drag operations here for setup</p>';
        if (testCase.setupOperations && testCase.setupOperations.length > 0) {
            setupZone.innerHTML = '';
            testCase.setupOperations.forEach((op, idx) => {
                const opId = Date.now() + idx;
                const functionData = op.functionId ? { id: op.functionId, functionId: op.functionId, name: op.functionName || 'Function' } : null;
                const opElement = createOperationElement(opId, op.operationType, 'setup', functionData);
                
                // Update displayed values
                if (op.delay) document.getElementById(`op-retry-delay-${opId}`).textContent = op.delay;
                if (op.retryCount) document.getElementById(`op-retry-${opId}`).textContent = op.retryCount;
                
                setupZone.appendChild(opElement);
            });
        }
        
        // Load Teardown Operations
        const teardownZone = document.getElementById('teardownZone');
        teardownZone.innerHTML = '<p class="zone-placeholder">Drag operations here for teardown</p>';
        if (testCase.teardownOperations && testCase.teardownOperations.length > 0) {
            teardownZone.innerHTML = '';
            testCase.teardownOperations.forEach((op, idx) => {
                const opId = Date.now() + 10000 + idx;
                const functionData = op.functionId ? { id: op.functionId, functionId: op.functionId, name: op.functionName || 'Function' } : null;
                const opElement = createOperationElement(opId, op.operationType, 'teardown', functionData);
                
                // Update displayed values
                if (op.delay) document.getElementById(`op-retry-delay-${opId}`).textContent = op.delay;
                if (op.retryCount) document.getElementById(`op-retry-${opId}`).textContent = op.retryCount;
                
                teardownZone.appendChild(opElement);
            });
        }
        
        // Load Steps
        const stepsZone = document.getElementById('stepsZone');
        stepsZone.innerHTML = '<p class="zone-placeholder">Drag steps here</p>';
        if (testCase.steps && testCase.steps.length > 0) {
            stepsZone.innerHTML = '';
            testCase.steps.forEach((step, stepIndex) => {
                const stepId = Date.now() + stepIndex * 100000;
                const stepElement = createStepElement(stepId);
                
                // Append step to DOM first so we can access its elements
                stepsZone.appendChild(stepElement);
                
                // Set step data using span elements
                const stepNameEl = document.getElementById(`step-name-${stepId}`);
                const stepDescEl = document.getElementById(`step-desc-${stepId}`);
                const stepDelayEl = document.getElementById(`step-delay-${stepId}`);
                const stepRetryEl = document.getElementById(`step-retry-${stepId}`);
                
                if (stepNameEl) stepNameEl.textContent = step.name || `Step ${stepIndex + 1}`;
                if (stepDescEl && step.description) stepDescEl.textContent = step.description;
                if (stepDelayEl && step.delay) stepDelayEl.textContent = step.delay;
                if (stepRetryEl && step.retryCount) stepRetryEl.textContent = step.retryCount;
                
                // Load Actions for this step
                const actionsZone = document.getElementById(`step-${stepId}-actions`);
                if (step.actions && step.actions.length > 0 && actionsZone) {
                    actionsZone.innerHTML = '';
                    step.actions.forEach((action, actionIndex) => {
                        const actionId = stepId + actionIndex * 1000;
                        const actionElement = createActionElement(actionId);
                        
                        // Append action to DOM first
                        actionsZone.appendChild(actionElement);
                        
                        // Set action data using span elements
                        const actionNameEl = document.getElementById(`action-name-${actionId}`);
                        const actionDescEl = document.getElementById(`action-desc-${actionId}`);
                        const actionDelayEl = document.getElementById(`action-delay-before-${actionId}`);
                        const actionRetryEl = document.getElementById(`action-retry-${actionId}`);
                        
                        if (actionNameEl) actionNameEl.textContent = action.name || `Action ${actionIndex + 1}`;
                        if (actionDescEl && action.description) actionDescEl.textContent = action.description;
                        if (actionDelayEl && action.delay) actionDelayEl.textContent = action.delay;
                        if (actionRetryEl && action.retryCount) actionRetryEl.textContent = action.retryCount;
                        
                        // Load Pre-Conditions
                        const preZone = document.getElementById(`action-${actionId}-pre`);
                        if (action.preConditions && action.preConditions.length > 0 && preZone) {
                            preZone.innerHTML = '';
                            action.preConditions.forEach((op, opIdx) => {
                                const opId = actionId * 1000 + opIdx;
                                const functionData = op.functionId ? { id: op.functionId, functionId: op.functionId, name: op.functionName || 'Function' } : null;
                                const opElement = createOperationElement(opId, op.operationType, `action-${actionId}-pre`, functionData);
                                
                                preZone.appendChild(opElement);
                                
                                // Update displayed values after appending
                                const delayEl = document.getElementById(`op-retry-delay-${opId}`);
                                const retryEl = document.getElementById(`op-retry-${opId}`);
                                if (delayEl && op.delay) delayEl.textContent = op.delay;
                                if (retryEl && op.retryCount) retryEl.textContent = op.retryCount;
                            });
                        }
                        
                        // Load Operations
                        const opsZone = document.getElementById(`action-${actionId}-operations`);
                        if (action.operations && action.operations.length > 0 && opsZone) {
                            opsZone.innerHTML = '';
                            action.operations.forEach((op, opIdx) => {
                                const opId = actionId * 1000 + 100 + opIdx;
                                const functionData = op.functionId ? { id: op.functionId, functionId: op.functionId, name: op.functionName || 'Function' } : null;
                                const opElement = createOperationElement(opId, op.operationType, `action-${actionId}-operations`, functionData);
                                
                                opsZone.appendChild(opElement);
                                
                                // Update displayed values after appending
                                const delayEl = document.getElementById(`op-retry-delay-${opId}`);
                                const retryEl = document.getElementById(`op-retry-${opId}`);
                                if (delayEl && op.delay) delayEl.textContent = op.delay;
                                if (retryEl && op.retryCount) retryEl.textContent = op.retryCount;
                            });
                        }
                        
                        // Load Post-Conditions
                        const postZone = document.getElementById(`action-${actionId}-post`);
                        if (action.postConditions && action.postConditions.length > 0 && postZone) {
                            postZone.innerHTML = '';
                            action.postConditions.forEach((op, opIdx) => {
                                const opId = actionId * 1000 + 200 + opIdx;
                                const functionData = op.functionId ? { id: op.functionId, functionId: op.functionId, name: op.functionName || 'Function' } : null;
                                const opElement = createOperationElement(opId, op.operationType, `action-${actionId}-post`, functionData);
                                
                                postZone.appendChild(opElement);
                                
                                // Update displayed values after appending
                                const delayEl = document.getElementById(`op-retry-delay-${opId}`);
                                const retryEl = document.getElementById(`op-retry-${opId}`);
                                if (delayEl && op.delay) delayEl.textContent = op.delay;
                                if (retryEl && op.retryCount) retryEl.textContent = op.retryCount;
                            });
                        }
                    });
                }
            });
        }
        
        // Store the test case ID for updating
        document.getElementById('testCaseId').value = id;
        
        // Update UI for edit mode
        const editBanner = document.getElementById('editModeBanner');
        const editingName = document.getElementById('editingTestCaseName');
        const saveButton = document.getElementById('saveButton');
        const saveButtonText = document.getElementById('saveButtonText');
        const cancelButton = document.getElementById('cancelButton');
        const pageTitle = document.getElementById('pageTitle');
        
        if (editBanner) {
            editBanner.style.display = 'block';
            if (editingName) editingName.textContent = testCase.name;
        }
        
        if (saveButton) {
            saveButton.classList.add('edit-mode');
            if (saveButtonText) saveButtonText.textContent = 'Update Test Case';
        }
        
        if (cancelButton) {
            cancelButton.style.display = 'inline-flex';
        }
        
        if (pageTitle) {
            pageTitle.innerHTML = '<i class="fas fa-edit"></i> Edit Test Case';
        }
        
        // Update all counts
        updateAllCounts();
        
        console.log('Test case loaded successfully with all data');
        showNotification('✓ Test case loaded for editing!', 'success');
    } catch (error) {
        console.error('Error loading test case:', error);
        showNotification('⚠ Error loading test case', 'error');
    }
}

function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
        <span>${message}</span>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.animation = 'fadeOut 0.3s';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

window.onclick = function(event) {
    const editModal = document.getElementById('editModal');
    const functionModal = document.getElementById('functionModal');
    if (event.target == editModal) {
        closeEditModal();
    }
    if (event.target == functionModal) {
        closeFunctionModal();
    }
}
