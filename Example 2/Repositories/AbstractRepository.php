<?php

namespace App\Repositories;

use Illuminate\Database\Eloquent\Builder;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Pagination\LengthAwarePaginator;
use Illuminate\Support\Collection;

abstract class AbstractRepository
{
    /** @var Builder */
    protected $model;

    public function __construct()
    {
        $this->model = app($this->getModelClass());
    }

    abstract public function getModelClass(): string;

    public function getOneById(int $id): ?Model
    {
        $model = $this->model;

        return $model->findOrFail($id);
    }

    public function getOneByIdWithoutFail(int $id): ?Model
    {
        $model = $this->model;

        return $model->find($id);
    }

    public function getByIds(array $ids, $limit = null): Collection
    {
        return $this->model->whereIn($this->model->getKeyName(), $ids)->limit($limit)->get();
    }

    public function getAll(): Collection
    {
        return $this->model->all();
    }

    public function getPaginated($limit = 10): LengthAwarePaginator
    {
        return $this->model->paginate(10);
    }

    public function getLatest($limit = null): Collection
    {
        return $this->model->latest()->limit($limit)->get();
    }

    public function countAll()
    {
        return $this->model->count();
    }

    public function create(array $data)
    {
        return $this->model->create($data);
    }

    public function update(int $id, array $data)
    {
        $model = $this->getOneById($id);

        $model->update($data);

        return $model;
    }

    public function save(Model $model)
    {
        $model->save();

        return $model;
    }

    public function delete(Model $model): ?bool
    {
        return $model->delete();
    }
}
