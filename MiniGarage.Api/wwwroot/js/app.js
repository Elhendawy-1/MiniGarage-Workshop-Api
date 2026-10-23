/**
 * Mini Garage UI — Client Application
 * Communicates with ASP.NET Core 9 Web API (/api/cars)
 */

document.addEventListener('DOMContentLoaded', () => {
  // Application State
  let allCars = [];
  let currentFilter = 'all';
  let searchQuery = '';

  // DOM Elements
  const carsGrid = document.getElementById('carsGrid');
  const searchInput = document.getElementById('searchInput');
  const filterTabs = document.querySelectorAll('.tab-btn');
  const toastContainer = document.getElementById('toastContainer');
  
  // KPI Elements
  const kpiTotal = document.getElementById('kpiTotal');
  const kpiPending = document.getElementById('kpiPending');
  const kpiInProgress = document.getElementById('kpiInProgress');
  const kpiCompleted = document.getElementById('kpiCompleted');

  // Modals
  const registerModal = document.getElementById('registerModal');
  const editModal = document.getElementById('editModal');
  const deleteModal = document.getElementById('deleteModal');
  const registerForm = document.getElementById('registerForm');
  const editForm = document.getElementById('editForm');
  const deleteForm = document.getElementById('deleteForm');

  // ─────────────────────────────────────────────────────────────
  // Initial Load
  // ─────────────────────────────────────────────────────────────
  loadCars();
  detectProtocol();

  // ─────────────────────────────────────────────────────────────
  // API Fetch Operations
  // ─────────────────────────────────────────────────────────────
  async function loadCars() {
    try {
      carsGrid.innerHTML = `
        <div class="empty-state">
          <div class="empty-state-icon">⏳</div>
          <h3>Loading workshop records...</h3>
          <p>Connecting to ASP.NET Core API.</p>
        </div>`;

      const response = await fetch('/api/cars');
      if (!response.ok) {
        throw new Error(`Failed to fetch cars: ${response.status} ${response.statusText}`);
      }

      allCars = await response.json();
      updateKpis();
      renderCars();
    } catch (err) {
      console.error(err);
      carsGrid.innerHTML = `
        <div class="empty-state">
          <div class="empty-state-icon" style="color: var(--status-danger);">⚠️</div>
          <h3>Unable to connect to API</h3>
          <p>${escapeHtml(err.message)}</p>
          <button class="btn btn-secondary btn-sm" onclick="location.reload()">Retry Connection</button>
        </div>`;
      showToast('Error connecting to Mini Garage API', 'error');
    }
  }

  // ─────────────────────────────────────────────────────────────
  // Rendering & Filtering
  // ─────────────────────────────────────────────────────────────
  function updateKpis() {
    kpiTotal.textContent = allCars.length;
    kpiPending.textContent = allCars.filter(c => c.repairStatus === 0).length;
    kpiInProgress.textContent = allCars.filter(c => c.repairStatus === 1).length;
    kpiCompleted.textContent = allCars.filter(c => c.repairStatus === 2).length;
  }

  function getFilteredCars() {
    return allCars.filter(car => {
      // Status filter
      if (currentFilter !== 'all' && car.repairStatus !== parseInt(currentFilter, 10)) {
        return false;
      }
      // Search query filter
      if (searchQuery) {
        const q = searchQuery.toLowerCase();
        const matchesPlate = car.licensePlate.toLowerCase().includes(q);
        const matchesOwner = car.ownerName.toLowerCase().includes(q);
        const matchesModel = car.carModel.toLowerCase().includes(q);
        return matchesPlate || matchesOwner || matchesModel;
      }
      return true;
    });
  }

  function renderCars() {
    const filtered = getFilteredCars();

    if (filtered.length === 0) {
      carsGrid.innerHTML = `
        <div class="empty-state">
          <div class="empty-state-icon">🚗</div>
          <h3>No vehicles found</h3>
          <p>${allCars.length === 0 ? "No cars registered in the garage yet." : "No vehicles match the selected filter criteria."}</p>
          <button class="btn btn-primary btn-sm" id="emptyStateActionBtn">+ Register First Vehicle</button>
        </div>`;
      
      const emptyBtn = document.getElementById('emptyStateActionBtn');
      if (emptyBtn) emptyBtn.addEventListener('click', () => openModal(registerModal));
      return;
    }

    carsGrid.innerHTML = filtered.map(car => {
      const statusClass = getStatusClass(car.repairStatus);
      const formattedDate = new Date(car.createdAt).toLocaleDateString(undefined, {
        month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
      });

      return `
        <article class="car-card" data-id="${car.id}">
          <div>
            <div class="card-top">
              <div class="license-plate" aria-label="License plate">${escapeHtml(car.licensePlate)}</div>
              <span class="status-badge ${statusClass}">
                ● ${escapeHtml(car.repairStatusLabel)}
              </span>
            </div>

            <h3 class="vehicle-model">${escapeHtml(car.carModel)}</h3>
            <div class="owner-name">👤 ${escapeHtml(car.ownerName)}</div>
          </div>

          <div class="issue-box">
            <strong>Work Required:</strong><br>
            ${escapeHtml(car.problemDescription)}
          </div>

          <div class="card-footer">
            <span class="created-time" title="Registered timestamp">Reg: ${formattedDate}</span>
            <div class="card-actions">
              <button class="btn btn-secondary btn-sm edit-btn" data-id="${car.id}">Edit</button>
              <button class="btn btn-danger btn-sm delete-btn" data-id="${car.id}">Delete</button>
            </div>
          </div>
        </article>`;
    }).join('');

    // Attach Event Listeners to Card Action Buttons
    document.querySelectorAll('.edit-btn').forEach(btn => {
      btn.addEventListener('click', () => handleEditClick(btn.dataset.id));
    });
    document.querySelectorAll('.delete-btn').forEach(btn => {
      btn.addEventListener('click', () => handleDeleteClick(btn.dataset.id));
    });
  }

  function getStatusClass(status) {
    switch (status) {
      case 0: return 'status-pending';
      case 1: return 'status-inprogress';
      case 2: return 'status-completed';
      default: return 'status-pending';
    }
  }

  // ─────────────────────────────────────────────────────────────
  // Search & Filter Events
  // ─────────────────────────────────────────────────────────────
  searchInput.addEventListener('input', (e) => {
    searchQuery = e.target.value.trim();
    renderCars();
  });

  filterTabs.forEach(tab => {
    tab.addEventListener('click', () => {
      filterTabs.forEach(t => {
        t.classList.remove('active');
        t.setAttribute('aria-selected', 'false');
      });
      tab.classList.add('active');
      tab.setAttribute('aria-selected', 'true');
      currentFilter = tab.dataset.filter;
      renderCars();
    });
  });

  // ─────────────────────────────────────────────────────────────
  // Create Vehicle (POST /api/cars)
  // ─────────────────────────────────────────────────────────────
  registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const payload = {
      ownerName: document.getElementById('regOwner').value.trim(),
      carModel: document.getElementById('regModel').value.trim(),
      licensePlate: document.getElementById('regPlate').value.trim(),
      problemDescription: document.getElementById('regProblem').value.trim(),
      repairStatus: parseInt(document.getElementById('regStatus').value, 10)
    };

    try {
      const response = await fetch('/api/cars', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(extractErrorMessage(errorData));
      }

      const created = await response.json();
      allCars.unshift(created);
      updateKpis();
      renderCars();
      closeModal(registerModal);
      registerForm.reset();
      showToast(`Vehicle ${created.licensePlate} registered successfully!`, 'success');
    } catch (err) {
      showToast(err.message, 'error');
    }
  });

  // ─────────────────────────────────────────────────────────────
  // Edit Vehicle (PUT /api/cars/{id})
  // ─────────────────────────────────────────────────────────────
  function handleEditClick(id) {
    const car = allCars.find(c => c.id === parseInt(id, 10));
    if (!car) return;

    document.getElementById('editCarId').value = car.id;
    document.getElementById('editOwner').value = car.ownerName;
    document.getElementById('editModel').value = car.carModel;
    document.getElementById('editPlate').value = car.licensePlate;
    document.getElementById('editProblem').value = car.problemDescription;
    document.getElementById('editStatus').value = car.repairStatus;

    openModal(editModal);
  }

  editForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = parseInt(document.getElementById('editCarId').value, 10);

    const payload = {
      ownerName: document.getElementById('editOwner').value.trim(),
      carModel: document.getElementById('editModel').value.trim(),
      licensePlate: document.getElementById('editPlate').value.trim(),
      problemDescription: document.getElementById('editProblem').value.trim(),
      repairStatus: parseInt(document.getElementById('editStatus').value, 10)
    };

    try {
      const response = await fetch(`/api/cars/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(extractErrorMessage(errorData));
      }

      const updated = await response.json();
      const idx = allCars.findIndex(c => c.id === id);
      if (idx !== -1) {
        allCars[idx] = updated;
      }

      updateKpis();
      renderCars();
      closeModal(editModal);
      showToast(`Vehicle ${updated.licensePlate} updated!`, 'success');
    } catch (err) {
      showToast(err.message, 'error');
    }
  });

  // ─────────────────────────────────────────────────────────────
  // Delete Vehicle (DELETE /api/cars/{id} with X-Admin-Key)
  // ─────────────────────────────────────────────────────────────
  function handleDeleteClick(id) {
    const car = allCars.find(c => c.id === parseInt(id, 10));
    if (!car) return;

    document.getElementById('deleteCarId').value = car.id;
    document.getElementById('deleteCarInfo').textContent = `${car.carModel} (${car.licensePlate})`;
    document.getElementById('adminKeyInput').value = '';
    openModal(deleteModal);
  }

  deleteForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = parseInt(document.getElementById('deleteCarId').value, 10);
    const adminKey = document.getElementById('adminKeyInput').value.trim();

    try {
      const response = await fetch(`/api/cars/${id}`, {
        method: 'DELETE',
        headers: {
          'X-Admin-Key': adminKey
        }
      });

      if (response.status === 401) {
        throw new Error('Unauthorized: Invalid X-Admin-Key provided.');
      }
      if (!response.ok && response.status !== 204) {
        throw new Error(`Delete failed: ${response.status} ${response.statusText}`);
      }

      allCars = allCars.filter(c => c.id !== id);
      updateKpis();
      renderCars();
      closeModal(deleteModal);
      showToast('Vehicle deleted from workshop records', 'success');
    } catch (err) {
      showToast(err.message, 'error');
    }
  });

  // ─────────────────────────────────────────────────────────────
  // Modal Utilities & Keyboard Accessibility
  // ─────────────────────────────────────────────────────────────
  document.getElementById('openRegisterModalBtn').addEventListener('click', () => {
    openModal(registerModal);
  });

  document.querySelectorAll('[data-close-modal]').forEach(btn => {
    btn.addEventListener('click', () => {
      const modalId = btn.dataset.closeModal;
      closeModal(document.getElementById(modalId));
    });
  });

  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      [registerModal, editModal, deleteModal].forEach(m => {
        if (m.classList.contains('open')) closeModal(m);
      });
    }
  });

  function openModal(modal) {
    modal.classList.add('open');
    const firstInput = modal.querySelector('input:not([type="hidden"]), select, textarea');
    if (firstInput) setTimeout(() => firstInput.focus(), 50);
  }

  function closeModal(modal) {
    modal.classList.remove('open');
  }

  // ─────────────────────────────────────────────────────────────
  // Protocol & Toast Notifications
  // ─────────────────────────────────────────────────────────────
  function detectProtocol() {
    const isHttps = window.location.protocol === 'https:';
    const textEl = document.getElementById('protocolText');
    if (textEl) {
      textEl.textContent = isHttps ? 'HTTPS / HTTP/3 Ready' : 'HTTP/1.1';
    }
  }

  function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toastContainer.appendChild(toast);

    setTimeout(() => {
      toast.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
      toast.style.opacity = '0';
      toast.style.transform = 'translateY(10px)';
      setTimeout(() => toast.remove(), 300);
    }, 4000);
  }

  function extractErrorMessage(errObj) {
    if (errObj.message) return errObj.message;
    if (errObj.errors) {
      const firstKey = Object.keys(errObj.errors)[0];
      if (firstKey && errObj.errors[firstKey].length > 0) {
        return errObj.errors[firstKey][0];
      }
    }
    return errObj.title || 'An error occurred during request.';
  }

  function escapeHtml(str) {
    if (!str) return '';
    return String(str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }
});
