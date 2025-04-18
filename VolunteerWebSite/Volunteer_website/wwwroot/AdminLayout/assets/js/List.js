// Đoạn mã JavaScript được sử dụng để xử lý các tương tác chọn tình nguyện viên 
// trong danh sách, tương tự như cách Gmail cho phép chọn email.

document.addEventListener('DOMContentLoaded', function() {
    const selectAllCheckbox = document.getElementById('select-all');
    const volunteerCheckboxes = document.querySelectorAll('input[name="inline-select"]');
    const bulkActions = document.querySelector('.bulk-actions');
    const selectedCount = document.querySelector('.selected-count');
    
    // Handle select all checkbox
    selectAllCheckbox.addEventListener('change', function() {
      const isChecked = this.checked;
      
      volunteerCheckboxes.forEach(checkbox => {
        checkbox.checked = isChecked;
        const row = checkbox.closest('tr');
        if (isChecked) {
          row.classList.add('selected-row');
        } else {
          row.classList.remove('selected-row');
        }
      });
      
      updateBulkActionsVisibility();
    });
    
    // Handle individual checkboxes
    volunteerCheckboxes.forEach(checkbox => {
      checkbox.addEventListener('change', function() {
        const row = this.closest('tr');
        if (this.checked) {
          row.classList.add('selected-row');
        } else {
          row.classList.remove('selected-row');
          selectAllCheckbox.checked = false;
        }
        
        updateBulkActionsVisibility();
      });
    });
    
    // Update bulk actions visibility
    function updateBulkActionsVisibility() {
      const checkedBoxes = document.querySelectorAll('input[name="inline-select"]:checked');
      
      if (checkedBoxes.length > 0) {
        bulkActions.classList.add('visible');
        selectedCount.classList.remove('d-none');
        selectedCount.querySelector('span').textContent = `${checkedBoxes.length} selected`;
      } else {
        bulkActions.classList.remove('visible');
        selectedCount.classList.add('d-none');
      }
    }
  });